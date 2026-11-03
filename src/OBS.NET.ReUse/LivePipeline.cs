using OBS.NET.Plugin;
using ReuseWinCli.Enhancement;

namespace OBS.NET.ReUse;

/// <summary>
/// OBS の音声スレッドと推論スレッドの橋渡し。
/// 音声スレッドは届いた音声（モノラルにまとめたもの）を入力のリングに書き、決まった遅れ <see cref="DelaySamples"/> の位置の補正済みの音声を受け取る。
/// 推論スレッドは入力のリングを読み、モデルのサンプリングレートに変換して <see cref="StreamingEnhancer"/> で補正し、戻して出力のリングに書く。
/// どちらのリングも入力の先頭からのサンプル番号で読み書きし、出力の n サンプル目は入力の n サンプル目を補正したものになる。
/// 推論が遅れて補正済みの音声が間に合わない区間は、元の音声をそのまま使う（遅れは変わらない）
/// </summary>
internal sealed class LivePipeline : IDisposable
{
    // 2^18 サンプル（48kHz で約 5.4 秒）。遅れと推論の滞りの分を十分に持てる
    const int Capacity = 1 << 18;
    const int Mask = Capacity - 1;
    // OBS が音声を渡す間隔（最大 1024 サンプル程度）と推論の時間のばらつきの分
    const double SafetySeconds = 0.03;

    readonly int obsRate;
    readonly float[] dry = new float[Capacity];
    readonly float[] wet = new float[Capacity];
    readonly AutoResetEvent signal = new(false);
    readonly Thread worker;
    readonly Lock wetGate = new();

    // 音声スレッドだけが書く
    long inputCount;
    // 推論スレッドが書き、wetGate の中で読む。補正済みの区間 [wetStart, wetEnd)
    long wetStart;
    long wetEnd;

    // 補正済みの音声が間に合わず、元の音声を出したサンプル数（音声スレッドだけが書く）
    long fallbackSamples;

    volatile ModelSettings requested;
    volatile bool stopping;
    volatile bool failed;
    volatile int delaySamples;

    public LivePipeline(int obsRate, int modelRate)
    {
        this.obsRate = obsRate;
        requested = ModelSettings.For(modelRate);
        delaySamples = GetDelaySamples(requested);
        worker = new Thread(Run) { IsBackground = true, Name = "RE-USE inference", Priority = ThreadPriority.AboveNormal };
        worker.Start();
    }

    /// <summary>推論器を用意できなかったか、推論中に失敗した。音声はそのまま通す</summary>
    public bool Failed => failed;

    /// <summary>入力から出力までの遅れ（OBS のサンプリングレートでのサンプル数）</summary>
    public int DelaySamples => delaySamples;

    public int ObsRate => obsRate;

    /// <summary>補正済みの音声が間に合わず、元の音声を出したサンプル数（推論器の準備中と、設定を変えた直後を含む）</summary>
    public long FallbackSamples => fallbackSamples;

    /// <summary>モデルのサンプリングレートを変える。推論スレッドが次に入力を読むときに、その位置から補正をやり直す</summary>
    public void SetModelRate(int modelRate)
    {
        if (requested.ModelRate == modelRate)
            return;
        requested = ModelSettings.For(modelRate);
        delaySamples = GetDelaySamples(requested);
    }

    /// <summary>
    /// 音声スレッドから呼ぶ。mono（frames サンプル）を入力に足し、DelaySamples だけ前の位置の音声で output を埋める
    /// </summary>
    /// <param name="wetMix">補正済みの音声の割合（0 から 1）</param>
    public void Process(ReadOnlySpan<float> mono, Span<float> output, float wetMix)
    {
        var start = inputCount;
        for (var i = 0; i < mono.Length; i++)
            dry[(start + i) & Mask] = mono[i];
        var end = start + mono.Length;
        Volatile.Write(ref inputCount, end);
        signal.Set();

        long wetFrom, wetTo;
        lock (wetGate)
            (wetFrom, wetTo) = (wetStart, wetEnd);

        var first = end - delaySamples - output.Length;
        for (var i = 0; i < output.Length; i++)
        {
            var index = first + i;
            if (index < 0)
            {
                output[i] = 0;
                continue;
            }
            var original = dry[index & Mask];
            if (index >= wetFrom && index < wetTo)
            {
                output[i] = original + wetMix * (wet[index & Mask] - original);
            }
            else
            {
                output[i] = original;
                fallbackSamples++;
            }
        }
    }

    void Run()
    {
        ReUseEngine engine;
        try
        {
            engine = SharedEngine.Acquire();
        }
        catch (Exception ex)
        {
            ObsPlugin.LogError(Localization.Get("log_engine_failed", ex.Message));
            failed = true;
            return;
        }

        try
        {
            var bypass = new BypassEnhancer(engine);
            var warmed = new HashSet<int>();
            Session? stream = null;
            var chunk = new float[Capacity / 2];
            while (!stopping)
            {
                signal.WaitOne(50);
                if (stopping)
                    break;

                var settings = requested;
                var end = Volatile.Read(ref inputCount);
                if (stream is null || stream.Settings != settings || end - stream.ReadPosition > Capacity / 2)
                {
                    // GPU の最初の推論はシェーダーの準備で遅いので、本番の前に 1 度流しておく
                    if (warmed.Add(settings.ModelRate))
                        WarmUp(engine, settings);
                    end = Volatile.Read(ref inputCount);
                    stream = new Session(settings, obsRate, bypass, end);
                    lock (wetGate)
                        wetStart = wetEnd = end;
                }

                var count = (int)(end - stream.ReadPosition);
                if (count == 0)
                    continue;

                // 1 ホップ分以上たまったら、追いつくまで推論を飛ばして元の音声を通す（StreamingEnhancer のクロスフェードでつながる）
                var hop = settings.Streaming.HopSamples * (long)obsRate / settings.ModelRate;
                bypass.Active = bypass.Active ? count > hop / 2 : count > hop;

                for (var i = 0; i < count; i++)
                    chunk[i] = dry[(stream.ReadPosition + i) & Mask];
                stream.ReadPosition = end;

                var enhanced = stream.Process(chunk.AsSpan(0, count));
                var writeAt = stream.Base + stream.Written;
                for (var i = 0; i < enhanced.Length; i++)
                    wet[(writeAt + i) & Mask] = enhanced[i];
                stream.Written += enhanced.Length;
                lock (wetGate)
                    wetEnd = stream.Base + stream.Written;
            }
        }
        catch (Exception ex)
        {
            ObsPlugin.LogError(Localization.Get("log_inference_failed", ex.Message));
            failed = true;
        }
        finally
        {
            SharedEngine.Release();
        }
    }

    static void WarmUp(ReUseEngine engine, ModelSettings settings)
    {
        var random = new Random(1);
        var samples = new float[settings.Streaming.WindowSamples];
        for (var i = 0; i < samples.Length; i++)
            samples[i] = (float)(random.NextDouble() - 0.5) * 0.01f;
        engine.Enhance(samples, settings.ModelRate);
    }

    int GetDelaySamples(ModelSettings settings)
    {
        // 補正の遅れ（ホップ + 先読み）と、1 回の推論の時間の分を待つ。変換の遅れは数ミリ秒
        var streaming = settings.Streaming;
        var seconds = (double)(streaming.HopSamples + streaming.LookaheadSamples) / settings.ModelRate + settings.InferenceSeconds + SafetySeconds;
        var resampler = settings.ModelRate == obsRate ? 0 : 2 * 64.0 / settings.ModelRate;
        return (int)Math.Ceiling((seconds + resampler) * obsRate);
    }

    public void Dispose()
    {
        // 推論器の読み込み中（数秒）は待たない。推論スレッドは stopping を見て抜け、推論器の参照を返す
        stopping = true;
        signal.Set();
        worker.Join(1000);
    }

    /// <summary>
    /// モデルのサンプリングレートごとの窓の組み方と、1 回の推論に見込む時間。評価（reuse-win-cli の StreamingEvaluationTest）で実時間に収まった組み合わせ。
    /// 44.1kHz と 48kHz は計算量がサンプリングレートに比例して重いので、過去の文脈を短くして（音質もこの方がよかった）窓を小さくする
    /// </summary>
    sealed record ModelSettings(int ModelRate, StreamingSettings Streaming, double InferenceSeconds)
    {
        public static ModelSettings For(int modelRate)
        {
            var (context, hop, lookahead, inference) = modelRate switch
            {
                16000 => (1.0, 0.2, 0.2, 0.2),
                24000 => (1.0, 0.3, 0.2, 0.3),
                >= 44100 => (0.3, 0.4, 0.2, 0.35),
                _ => throw new ArgumentOutOfRangeException(nameof(modelRate)),
            };
            return new(modelRate, StreamingSettings.FromSeconds(modelRate, context, hop, lookahead, Math.Min(hop, lookahead) / 2), inference);
        }
    }

    /// <summary>入力の Base サンプル目から始めた、変換と補正の一続きの流れ</summary>
    sealed class Session(ModelSettings settings, int obsRate, ISpeechEnhancer enhancer, long start)
    {
        public ModelSettings Settings { get; } = settings;
        public long Base { get; } = start;
        public long ReadPosition { get; set; } = start;
        public long Written { get; set; }
        readonly StreamingResampler? down = settings.ModelRate == obsRate ? null : new(obsRate, settings.ModelRate);
        readonly StreamingEnhancer streamer = new(enhancer, settings.ModelRate, settings.Streaming);
        readonly StreamingResampler? up = settings.ModelRate == obsRate ? null : new(settings.ModelRate, obsRate);

        /// <summary>OBS のレートの入力を補正し、OBS のレートで返す。モデルと同じレートなら変換しない</summary>
        public float[] Process(ReadOnlySpan<float> input)
        {
            var enhanced = streamer.Process(down is null ? input : down.Process(input));
            return up is null ? enhanced : up.Process(enhanced);
        }
    }

    /// <summary>推論が遅れているあいだ、GPU を使わずに入力をそのまま返す</summary>
    sealed class BypassEnhancer(ISpeechEnhancer inner) : ISpeechEnhancer
    {
        public bool Active { get; set; }

        public int GetChunkSamples(int sampleRate) => inner.GetChunkSamples(sampleRate);

        public float[] Enhance(ReadOnlySpan<float> samples, int sampleRate, CancellationToken token)
            => Active ? samples.ToArray() : inner.Enhance(samples, sampleRate, token);
    }
}

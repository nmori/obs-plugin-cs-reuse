using OBS.NET.Native;
using OBS.NET.Plugin;

namespace OBS.NET.ReUse;

/// <summary>
/// 音声フィルター「AIノイズ除去（RE-USE）」。音声をモノラルにまとめて RE-USE で補正し、すべてのチャンネルに書き戻す。
/// 補正には未来の音声（先読み）が要るので、出力は決まった時間だけ遅れる。
/// 「映像と同期」が有効なら、出力のタイムスタンプを遅れの分だけ前にずらし、OBS に音声のバッファリングで映像と合わせてもらう
/// </summary>
public class ReUseFilter : ObsAudioFilter
{
    const string PropModelRate = "model_rate";
    const string PropMix = "mix";
    const string PropSync = "sync";
    // 品質の設定値。0 はモデルを OBS と同じレート（48kHz か 44.1kHz）で動かす
    const int FullBand = 0;

    LivePipeline? pipeline;
    int obsRate = 48000;
    int channels = 2;
    int modelRate = 24000;
    float wetMix = 1;
    bool sync = true;
    bool loggedFailure;
    long lastReportTicks = Environment.TickCount64;
    long reportedFallback;

    float[] mono = new float[1024];
    float[] output = new float[1024];

    public override string Id => "reuse_noise_filter";
    public override string DisplayName => Localization.Get("filter_name");

    protected override void OnCreate(ObsData settings)
    {
        var info = new ObsAudioInfo();
        if (ObsApi.GetAudioInfo(ref info) && info.SamplesPerSec > 0)
        {
            obsRate = (int)info.SamplesPerSec;
            channels = GetChannelCount(info.Speakers);
        }
        LoadSettings(settings);
        pipeline = new LivePipeline(obsRate, modelRate);
    }

    protected override void OnDestroy()
    {
        pipeline?.Dispose();
        pipeline = null;
    }

    protected override void GetDefaults(ObsData settings)
    {
        ObsDataHelper.SetDefaultInt(settings, PropModelRate, 24000);
        ObsDataHelper.SetDefaultInt(settings, PropMix, 100);
        ObsDataHelper.SetDefaultBool(settings, PropSync, true);
    }

    protected override ObsProperties? GetProperties()
    {
        return new ObsPropertiesBuilder()
            .AddListInt(PropModelRate, Localization.Get("prop_model_rate"), ListType.List, prop =>
            {
                ObsApi.PropertyListAddInt(prop, Localization.Get("prop_model_rate_full"), FullBand);
                ObsApi.PropertyListAddInt(prop, Localization.Get("prop_model_rate_24k"), 24000);
                ObsApi.PropertyListAddInt(prop, Localization.Get("prop_model_rate_16k"), 16000);
            })
            .AddInt(PropMix, Localization.Get("prop_mix"), 0, 100)
            .AddBool(PropSync, Localization.Get("prop_sync"))
            .Build();
    }

    protected override void OnUpdate(ObsData settings)
    {
        LoadSettings(settings);
        pipeline?.SetModelRate(modelRate);
    }

    void LoadSettings(ObsData settings)
    {
        modelRate = ObsDataHelper.GetInt(settings, PropModelRate) switch
        {
            FullBand => obsRate,
            16000 => 16000,
            _ => 24000,
        };
        wetMix = Math.Clamp(ObsDataHelper.GetInt(settings, PropMix), 0, 100) / 100f;
        sync = ObsDataHelper.GetBool(settings, PropSync);
    }

    protected override unsafe IntPtr OnFilterAudio(IntPtr audioDataPtr)
    {
        var current = pipeline;
        if (current is null || audioDataPtr == IntPtr.Zero)
            return audioDataPtr;
        if (current.Failed)
        {
            // 推論器がない（GPU がないなど）ときは何もしない。理由はログに出ている
            if (!loggedFailure)
            {
                loggedFailure = true;
                ObsPlugin.LogWarning(Localization.Get("log_passthrough"));
            }
            return audioDataPtr;
        }

        var audio = (ObsAudioData*)audioDataPtr;
        var frames = (int)audio->Frames;
        if (frames == 0)
            return audioDataPtr;

        var planes = new Span<IntPtr>(&audio->Data, 8);
        var count = 0;
        while (count < channels && planes[count] != IntPtr.Zero)
            count++;
        if (count == 0)
            return audioDataPtr;

        if (mono.Length < frames)
        {
            mono = new float[frames];
            output = new float[frames];
        }

        var mixed = mono.AsSpan(0, frames);
        mixed.Clear();
        for (var c = 0; c < count; c++)
        {
            var plane = new ReadOnlySpan<float>((void*)planes[c], frames);
            for (var i = 0; i < frames; i++)
                mixed[i] += plane[i];
        }
        if (count > 1)
        {
            var scale = 1f / count;
            for (var i = 0; i < frames; i++)
                mixed[i] *= scale;
        }

        var result = output.AsSpan(0, frames);
        current.Process(mixed, result, wetMix);
        for (var c = 0; c < count; c++)
            result.CopyTo(new Span<float>((void*)planes[c], frames));
        ReportFallback(current);

        if (sync)
        {
            // 出力の先頭は DelaySamples だけ前に届いた音声なので、そのときの時刻を付ける
            var delay = (ulong)((long)current.DelaySamples * 1_000_000_000L / current.ObsRate);
            audio->Timestamp = audio->Timestamp > delay ? audio->Timestamp - delay : 0;
        }
        return audioDataPtr;
    }

    /// <summary>推論が間に合わなかった時間を 30 秒ごとにログに出す（推論器の準備中の分も含む）</summary>
    void ReportFallback(LivePipeline current)
    {
        var now = Environment.TickCount64;
        if (now - lastReportTicks < 30_000)
            return;
        lastReportTicks = now;
        var fallback = current.FallbackSamples;
        if (fallback > reportedFallback)
            ObsPlugin.LogWarning(Localization.Get("log_fallback", (double)(fallback - reportedFallback) / current.ObsRate));
        reportedFallback = fallback;
    }

    static int GetChannelCount(SpeakerLayout layout) => layout switch
    {
        SpeakerLayout.Mono => 1,
        SpeakerLayout.Stereo => 2,
        SpeakerLayout.Layout2Point1 => 3,
        SpeakerLayout.Layout4Point0 => 4,
        SpeakerLayout.Layout4Point1 => 5,
        SpeakerLayout.Layout5Point1 => 6,
        SpeakerLayout.Layout7Point1 => 8,
        _ => 2,
    };
}

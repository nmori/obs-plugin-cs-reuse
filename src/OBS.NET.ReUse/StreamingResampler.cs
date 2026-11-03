namespace OBS.NET.ReUse;

/// <summary>
/// 少しずつ届く音声のサンプリングレートを変換する（窓付き sinc の多相フィルター）。
/// 出力の j サンプル目は、入力の時刻 j × inRate / outRate サンプル目に当たる。位置がずれないので、変換して戻すと元の位置に揃う。
/// 入力の先頭より前は 0 とみなす。出力は入力より <see cref="HalfTaps"/> サンプル分だけ遅れて出てくる
/// </summary>
internal sealed class StreamingResampler
{
    const int ZeroCrossings = 16;
    // 折り返しを防ぐため、出力のナイキスト周波数より少し下で切る
    const double CutoffRatio = 0.92;

    readonly int up;
    readonly int down;
    readonly float[][] phases;

    // 入力の保持分。buffer[0] が入力の bufferStart サンプル目（負なら先頭より前の 0）
    float[] buffer = new float[4096];
    int bufferLength;
    long bufferStart;
    long received;
    long produced;

    public StreamingResampler(int inRate, int outRate)
    {
        var gcd = Gcd(inRate, outRate);
        up = outRate / gcd;
        down = inRate / gcd;
        var cutoff = Math.Min(1.0, (double)outRate / inRate) * CutoffRatio;
        HalfTaps = (int)Math.Ceiling(ZeroCrossings / cutoff);

        phases = new float[up][];
        for (var p = 0; p < up; p++)
        {
            var taps = new float[HalfTaps * 2];
            double sum = 0;
            for (var t = 0; t < taps.Length; t++)
            {
                // 入力の (base - HalfTaps + 1 + t) サンプル目と、出力の位置 base + p / up との距離
                var x = t - HalfTaps + 1 - (double)p / up;
                var value = cutoff * Sinc(cutoff * x) * Blackman(x / HalfTaps);
                taps[t] = (float)value;
                sum += value;
            }
            for (var t = 0; t < taps.Length; t++)
                taps[t] = (float)(taps[t] / sum);
            phases[p] = taps;
        }

        bufferStart = -HalfTaps;
        bufferLength = HalfTaps;
    }

    public int HalfTaps { get; }

    public float[] Process(ReadOnlySpan<float> input)
    {
        Append(input);

        // 出力 j は入力 base = floor(j × down / up) の前後（base - HalfTaps + 1 から base + HalfTaps まで）を使うので、
        // base + HalfTaps < received、つまり j × down ≤ (received - HalfTaps) × up - 1 まで出せる
        var limit = (received - HalfTaps) * up - 1;
        var available = limit < 0 ? 0 : limit / down + 1;
        var count = (int)Math.Max(0, available - produced);
        var output = new float[count];
        for (var i = 0; i < count; i++)
        {
            var position = (produced + i) * down;
            var baseIndex = position / up;
            var taps = phases[position % up];
            var x = buffer.AsSpan((int)(baseIndex - HalfTaps + 1 - bufferStart), taps.Length);
            float sum = 0;
            for (var t = 0; t < taps.Length; t++)
                sum += x[t] * taps[t];
            output[i] = sum;
        }
        produced += count;
        Trim(produced * down / up - HalfTaps + 1);
        return output;
    }

    void Append(ReadOnlySpan<float> input)
    {
        if (bufferLength + input.Length > buffer.Length)
            Array.Resize(ref buffer, Math.Max(buffer.Length * 2, bufferLength + input.Length));
        input.CopyTo(buffer.AsSpan(bufferLength));
        bufferLength += input.Length;
        received += input.Length;
    }

    void Trim(long keepFrom)
    {
        var drop = (int)Math.Clamp(keepFrom - bufferStart, 0, bufferLength);
        if (drop == 0)
            return;
        buffer.AsSpan(drop, bufferLength - drop).CopyTo(buffer);
        bufferLength -= drop;
        bufferStart += drop;
    }

    static double Sinc(double x) => Math.Abs(x) < 1e-12 ? 1 : Math.Sin(Math.PI * x) / (Math.PI * x);

    /// <param name="x">-1 から 1。範囲の外は 0</param>
    static double Blackman(double x)
    {
        if (Math.Abs(x) >= 1)
            return 0;
        var phase = Math.PI * (x + 1);
        return 0.42 - 0.5 * Math.Cos(phase) + 0.08 * Math.Cos(2 * phase);
    }

    static int Gcd(int a, int b)
    {
        while (b != 0)
            (a, b) = (b, a % b);
        return a;
    }
}

using System;
using System.Threading;

namespace ReuseWinCli.Enhancement
{
    // reuse-win-cli の ChunkedEnhancer.cs から、推論の差し替え口だけを取り出したもの
    internal interface ISpeechEnhancer
    {
        /// <summary>1 回で処理するサンプル数（偶数）</summary>
        int GetChunkSamples(int sampleRate);

        /// <summary>モノラルの音声を補正して、同じ長さで返す。別々のスレッドから同時に呼ばれる。中止されたら OperationCanceledException</summary>
        float[] Enhance(ReadOnlySpan<float> samples, int sampleRate, CancellationToken token);
    }
}

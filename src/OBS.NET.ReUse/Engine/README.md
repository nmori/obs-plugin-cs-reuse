# Engine

RE-USE の推論エンジン。reuse-win-cli（https://github.com/nmori/reuse-win-cli、MIT）の
`src/ReuseWinCli/Enhancement`、`EmbeddedResources.cs`、`Shaders`、`Shaders.targets`、`Model.targets` をコピーしたもの。
取り込んだ時点のコミットは efa3828（StreamingEnhancer.cs は未コミットの版）。

`ChunkedEnhancer.cs` は CLI の音声入出力に依存するので取り込まず、`ISpeechEnhancer` だけを `ISpeechEnhancer.cs` に置いた。
エンジンを直すときは reuse-win-cli 側を直してからコピーし直す。

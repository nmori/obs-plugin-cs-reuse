# AIノイズ除去（RE-USE）フィルター

NVIDIA の音声補正モデル [RE-USE](https://huggingface.co/nvidia/NVIDIA-NemotronLabs-RE-USE) で、声の雑音や残響を取り除く OBS の音声フィルターです。
推論は Direct3D 11 のコンピュートシェーダーで GPU を使って行います（エンジンは改変版 [reuse-win-cli](https://github.com/nmori/reuse-win-cli) から取り込み）。
本家はこちら→[reuse-win-cli] (https://github.com/manju-summoner/reuse-win-cli)

## 使い方

1. `obs-reuse-filter.dll` を `C:\Program Files\obs-studio\obs-plugins\64bit` に置く
2. マイクなどの音声ソースの「フィルタ」→ 音声フィルタの「+」→「AIノイズ除去（RE-USE）」

| 設定 | 内容 |
|---|---|
| 品質 | フル帯域（OBS と同じ 48kHz／44.1kHz で推論、遅延 約1.0秒、重い）／24kHz（12kHz までの帯域、遅延 約0.85秒）／16kHz（8kHz までの帯域、遅延 約0.65秒、軽い） |
| 効果の強さ | 補正した音と元の音の混ぜ具合（100% で補正した音だけ） |
| 映像と同期する | 出力のタイムスタンプを遅延の分だけ前にずらし、OBS の音声バッファリングで映像と合わせる（既定で有効） |

## 遅延について

RE-USE は前後の音声を見て補正するモデルなので、少し先の音声（先読み）が届くまで出力できません。
このフィルターは常に一定の時間（上の表の遅延）だけ遅らせて出力します。

- 「映像と同期する」が有効なら、OBS が全体の音声バッファリングを増やして映像と合わせます（ログに「adding ... milliseconds of audio buffering」と出ます）。
  OBS の詳細設定の「低遅延音声バッファリングモード」が有効だと、バッファリングを増やせずに音が途切れることがあります。
- 無効にした場合は音声だけが遅れるので、映像側に「映像の遅延（非同期）」フィルターなどで同じ時間の遅延をかけてください。

## 動作

- 音声はモノラルにまとめて補正し、すべてのチャンネルに同じ音を書き戻します（声向け）。
- OBS のサンプリングレート（48kHz／44.1kHz）からモデルのレートに変換して補正し、元のレートに戻します。フル帯域では変換しません。
- 1 回の推論の時間（RTX 4050 Laptop）: フル帯域 48kHz で約 240ms（ホップ 400ms ごと）、44.1kHz で約 220ms、24kHz で約 190ms（300ms ごと）、16kHz で約 115ms（200ms ごと）。
- 推論は別スレッドで行い、OBS の音声スレッドは待たせません。推論が間に合わない区間は元の音をそのまま出します。
  間に合わなかった時間は 30 秒ごとにログに出ます。多いときは 16kHz にしてください。
- 複数のソースに付けても、推論器（重みと GPU のバッファ）は 1 つを共有します。
- Direct3D 11 で使える GPU がない場合（CPU だけの WARP になる場合）は、何もせず音を通します。

## ビルド

```powershell
.\build.ps1
```

.NET 10 SDK、Visual Studio の C++ ツール（NativeAOT のリンク用）、Windows SDK（fxc.exe）が必要です。
初回は RE-USE の重みを Hugging Face から `.cache` に取ってきて、DLL に埋め込みます。

## ライセンス
- 本家[reuse-win-cli] (https://github.com/manju-summoner/reuse-win-cli)
- RE-USE の重み: OpenMDW-1.1（`THIRD-PARTY-NOTICES.txt`）
- 推論エンジン（reuse-win-cli 由来のコード）: MIT（`reuse-win-cli_LICENSE.txt`）

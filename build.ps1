# RE-USE ノイズ除去フィルターのビルド
# 使い方: .\build.ps1 [-Configuration Debug|Release]
# 初回は RE-USE の重み（38MB）を Hugging Face から .cache に取ってくる。シェーダーのコンパイルに Windows SDK の fxc.exe を使う

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

# NativeAOT のリンカーが vswhere.exe を探すので PATH に入れる
$env:PATH = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer;$env:PATH"

$project = Join-Path $PSScriptRoot "src\OBS.NET.ReUse\OBS.NET.ReUse.csproj"
dotnet publish $project -c $Configuration -r $Runtime --self-contained
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$publish = Join-Path $PSScriptRoot "src\OBS.NET.ReUse\bin\$Configuration\net10.0-windows\$Runtime\publish"
$out = Join-Path $PSScriptRoot "publish\$Runtime"
New-Item -ItemType Directory -Force $out | Out-Null
Copy-Item (Join-Path $publish "obs-reuse-filter.dll") $out -Force
Write-Host "出力: $out\obs-reuse-filter.dll" -ForegroundColor Green

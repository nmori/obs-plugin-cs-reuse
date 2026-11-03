using System.Runtime.InteropServices;

namespace OBS.NET.ReUse;

/// <summary>
/// 表示する文字列。Windows の表示言語が日本語なら日本語、それ以外は英語にする
/// （InvariantGlobalization なので CultureInfo では言語がわからない）
/// </summary>
internal static partial class Localization
{
    static readonly Dictionary<string, string> English = new()
    {
        ["plugin_name"] = "RE-USE Noise Filter",
        ["plugin_description"] = "AI speech enhancement (NVIDIA RE-USE) audio filter",
        ["filter_name"] = "AI Noise Removal (RE-USE)",
        ["prop_model_rate"] = "Quality",
        ["prop_model_rate_full"] = "Full band, same rate as OBS (48 / 44.1 kHz, delay about 1.0 s, heavy)",
        ["prop_model_rate_24k"] = "24 kHz (delay about 0.85 s)",
        ["prop_model_rate_16k"] = "16 kHz (delay about 0.65 s, lighter)",
        ["prop_mix"] = "Effect strength (%)",
        ["prop_sync"] = "Keep in sync with video (shift audio timestamps by the delay)",
        ["log_loaded"] = "{0} loaded",
        ["log_engine_ready"] = "[RE-USE] Inference engine ready on {0}",
        ["log_no_gpu"] = "No GPU usable by Direct3D 11 was found. RE-USE cannot run in real time on the CPU.",
        ["log_engine_failed"] = "[RE-USE] Could not start the inference engine: {0}",
        ["log_inference_failed"] = "[RE-USE] Inference failed: {0}",
        ["log_passthrough"] = "[RE-USE] Passing audio through unchanged because the inference engine is unavailable.",
        ["log_fallback"] = "[RE-USE] Inference could not keep up for {0:0.00} s in the last 30 s (original audio was used). Try 16 kHz.",
    };

    static readonly Dictionary<string, string> Japanese = new()
    {
        ["plugin_name"] = "RE-USE ノイズ除去フィルター",
        ["plugin_description"] = "AI で声を補正する音声フィルター（NVIDIA RE-USE）",
        ["filter_name"] = "AIノイズ除去（RE-USE）",
        ["prop_model_rate"] = "品質",
        ["prop_model_rate_full"] = "フル帯域・OBS と同じレート（48/44.1kHz、遅延 約1.0秒・重い）",
        ["prop_model_rate_24k"] = "24kHz（遅延 約0.85秒）",
        ["prop_model_rate_16k"] = "16kHz（遅延 約0.65秒・軽い）",
        ["prop_mix"] = "効果の強さ（%）",
        ["prop_sync"] = "映像と同期する（音声のタイムスタンプを遅延の分だけずらす）",
        ["log_loaded"] = "{0} を読み込みました",
        ["log_engine_ready"] = "[RE-USE] 推論に使う GPU: {0}",
        ["log_no_gpu"] = "Direct3D 11 で使える GPU が見つかりません。RE-USE は CPU では実時間に間に合いません。",
        ["log_engine_failed"] = "[RE-USE] 推論器を用意できませんでした: {0}",
        ["log_inference_failed"] = "[RE-USE] 推論に失敗しました: {0}",
        ["log_passthrough"] = "[RE-USE] 推論器が使えないので、音声をそのまま通します。",
        ["log_fallback"] = "[RE-USE] この 30 秒で {0:0.00} 秒分、推論が間に合わず元の音声を使いました。重い場合は 16kHz にしてください。",
    };

    static readonly Dictionary<string, string> Current = IsJapanese() ? Japanese : English;

    public static string Get(string key)
        => Current.TryGetValue(key, out var value) || English.TryGetValue(key, out value) ? value : $"[{key}]";

    public static string Get(string key, params object[] args) => string.Format(Get(key), args);

    // LANGID の下位 10 ビットが主言語。0x11 が日本語
    static bool IsJapanese() => (GetUserDefaultUILanguage() & 0x3FF) == 0x11;

    [LibraryImport("kernel32.dll")]
    private static partial ushort GetUserDefaultUILanguage();
}

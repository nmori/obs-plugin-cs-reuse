using OBS.NET.Plugin;

namespace OBS.NET.ReUse;

/// <summary>
/// RE-USE（NVIDIA の音声補正モデル）で声の雑音や残響を取り除く音声フィルターのプラグイン
/// </summary>
public class ReUsePlugin : ObsPlugin
{
    public override string Name => Localization.Get("plugin_name");
    public override string Author => "*nao* (N.Mori)";
    public override string Description => Localization.Get("plugin_description");

    public override bool Load()
    {
        RegisterFilter<ReUseFilter>();
        LogInfo(Localization.Get("log_loaded", Name));
        return true;
    }
}

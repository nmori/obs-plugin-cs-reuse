using OBS.NET.Plugin;
using ReuseWinCli.Enhancement;

namespace OBS.NET.ReUse;

/// <summary>
/// RE-USE の推論器をフィルターの間で共有する。重み（38MB）と GPU の作業用バッファを 1 つで済ませ、GPU の処理は推論器の中で 1 つずつ行われる
/// </summary>
internal static class SharedEngine
{
    static readonly Lock gate = new();
    static ReUseEngine? engine;
    static int references;

    /// <summary>推論器を作る（初回は重みの読み込みに数秒かかる）。GPU がなく WARP（CPU）になるときは、実時間に間に合わないので例外にする</summary>
    public static ReUseEngine Acquire()
    {
        lock (gate)
        {
            if (engine is null)
            {
                var created = ReUseEngine.Create(null, forceWarp: false);
                if (created.IsWarp)
                {
                    created.Dispose();
                    throw new InvalidOperationException(Localization.Get("log_no_gpu"));
                }
                ObsPlugin.LogInfo(Localization.Get("log_engine_ready", created.AdapterName));
                engine = created;
            }
            references++;
            return engine;
        }
    }

    public static void Release()
    {
        lock (gate)
        {
            if (--references > 0)
                return;
            engine?.Dispose();
            engine = null;
            references = 0;
        }
    }
}

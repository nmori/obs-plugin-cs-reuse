using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OBS.NET.Plugin;

namespace OBS.NET.ReUse;

/// <summary>
/// Native exports for the RE-USE noise filter OBS plugin module
/// These functions are called by OBS when the module is loaded
/// </summary>
public static class ModuleExports
{
    // Plugin metadata - English as default for international compatibility
    private const string PluginName = "RE-USE Noise Filter";
    private const string PluginDescription = "AI speech enhancement (NVIDIA RE-USE) audio filter";
    private const string PluginAuthor = "*nao*";

    // Module initializer runs when the module is first accessed
    [ModuleInitializer]
    internal static void Initialize()
    {
        // Use factory pattern for lazy initialization to avoid NativeAOT static constructor issues
        ObsModuleExports.SetPluginFactory(() => new ReUsePlugin());
    }

    /// <summary>
    /// Called by OBS to set the module pointer
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "obs_module_set_pointer")]
    public static void ObsModuleSetPointer(IntPtr obsModule)
    {
        ObsModuleExports.ModuleSetPointer(obsModule);
    }

    /// <summary>
    /// Called by OBS to get the API version
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "obs_module_ver")]
    public static uint ObsModuleVer()
    {
        return ObsModuleExports.ModuleVersion();
    }

    /// <summary>
    /// Called by OBS when the module is loaded
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "obs_module_load")]
    public static int ObsModuleLoad()
    {
        return ObsModuleExports.ModuleLoad() ? 1 : 0;
    }

    /// <summary>
    /// Called by OBS when the module is unloaded
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "obs_module_unload")]
    public static void ObsModuleUnload()
    {
        ObsModuleExports.ModuleUnload();
    }

    /// <summary>
    /// Called by OBS after all modules have been loaded
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "obs_module_post_load")]
    public static void ObsModulePostLoad()
    {
        ObsModuleExports.ModulePostLoad();
    }

    /// <summary>
    /// Returns the module name (UTF-8 encoded)
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "obs_module_name")]
    public static IntPtr ObsModuleName()
    {
        return ObsModuleExports.GetModuleNamePtr(PluginName);
    }

    /// <summary>
    /// Returns the module description (UTF-8 encoded)
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "obs_module_description")]
    public static IntPtr ObsModuleDescription()
    {
        return ObsModuleExports.GetModuleDescriptionPtr(PluginDescription);
    }

    /// <summary>
    /// Returns the module author (UTF-8 encoded)
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "obs_module_author")]
    public static IntPtr ObsModuleAuthor()
    {
        return ObsModuleExports.GetModuleAuthorPtr(PluginAuthor);
    }
}

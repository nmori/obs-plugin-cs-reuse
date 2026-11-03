using System.Runtime.InteropServices;

namespace OBS.NET.Plugin;

/// <summary>
/// Static class containing module export helpers for NativeAOT
/// </summary>
public static class ObsModuleExports
{
    private static ObsPlugin? _plugin;
    private static Func<ObsPlugin>? _pluginFactory;
    private static IntPtr _obsModule;

    // Cached UTF-8 string pointers (allocated once, never freed - OBS owns them)
    private static IntPtr _namePtr;
    private static IntPtr _descriptionPtr;
    private static IntPtr _authorPtr;

    /// <summary>
    /// Sets the plugin factory for lazy initialization.
    /// This avoids creating plugin instances in static constructors which can cause crashes.
    /// </summary>
    /// <param name="factory">A factory function that creates the plugin instance</param>
    public static void SetPluginFactory(Func<ObsPlugin> factory)
    {
        _pluginFactory = factory;
    }

    /// <summary>
    /// Sets the plugin instance to be used (legacy method, prefer SetPluginFactory)
    /// </summary>
    [Obsolete("Use SetPluginFactory for lazy initialization to avoid NativeAOT static constructor issues")]
    public static void SetPlugin(ObsPlugin plugin)
    {
        _plugin = plugin;
    }

    /// <summary>
    /// Gets or creates the plugin instance
    /// </summary>
    private static ObsPlugin? GetPlugin()
    {
        if (_plugin == null && _pluginFactory != null)
        {
            try
            {
                _plugin = _pluginFactory();
            }
            catch
            {
                // Silently fail - will return null and ModuleLoad will return false
                return null;
            }
        }
        return _plugin;
    }

    /// <summary>
    /// Implementation of obs_module_load
    /// </summary>
    public static bool ModuleLoad()
    {
        var plugin = GetPlugin();
        if (plugin == null)
        {
            return false;
        }

        try
        {
            plugin.Module = _obsModule;
            return plugin.Load();
        }
        catch (Exception ex)
        {
            try
            {
                ObsPlugin.LogError($"Failed to load plugin: {ex.Message}");
            }
            catch
            {
                // Ignore logging errors during early initialization
            }
            return false;
        }
    }

    /// <summary>
    /// Implementation of obs_module_unload
    /// </summary>
    public static void ModuleUnload()
    {
        try
        {
            _plugin?.Unload();
        }
        catch (Exception ex)
        {
            try
            {
                ObsPlugin.LogError($"Error during unload: {ex.Message}");
            }
            catch
            {
                // Ignore logging errors
            }
        }
        finally
        {
            ObsPlugin.FreePinnedResources();
        }
    }

    /// <summary>
    /// Implementation of obs_module_post_load
    /// </summary>
    public static void ModulePostLoad()
    {
        try
        {
            _plugin?.PostLoad();
        }
        catch (Exception ex)
        {
            try
            {
                ObsPlugin.LogError($"Error during post load: {ex.Message}");
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }

    /// <summary>
    /// Implementation of obs_module_set_pointer
    /// </summary>
    public static void ModuleSetPointer(IntPtr obsModule)
    {
        _obsModule = obsModule;
    }

    /// <summary>
    /// Gets the OBS API version this plugin was built against
    /// </summary>
    public static uint ModuleVersion()
    {
        // OBS Studio API version (30.0.0)
        return (30u << 24) | (0u << 16) | 0u;
    }

    /// <summary>
    /// Returns a cached UTF-8 pointer for the module name
    /// </summary>
    /// <param name="name">The module name string</param>
    /// <returns>Pointer to UTF-8 encoded string</returns>
    public static IntPtr GetModuleNamePtr(string name)
    {
        if (_namePtr == IntPtr.Zero)
        {
            _namePtr = ObsPlugin.StringToUtf8Ptr(name);
        }
        return _namePtr;
    }

    /// <summary>
    /// Returns a cached UTF-8 pointer for the module description
    /// </summary>
    /// <param name="description">The module description string</param>
    /// <returns>Pointer to UTF-8 encoded string</returns>
    public static IntPtr GetModuleDescriptionPtr(string description)
    {
        if (_descriptionPtr == IntPtr.Zero)
        {
            _descriptionPtr = ObsPlugin.StringToUtf8Ptr(description);
        }
        return _descriptionPtr;
    }

    /// <summary>
    /// Returns a cached UTF-8 pointer for the module author
    /// </summary>
    /// <param name="author">The module author string</param>
    /// <returns>Pointer to UTF-8 encoded string</returns>
    public static IntPtr GetModuleAuthorPtr(string author)
    {
        if (_authorPtr == IntPtr.Zero)
        {
            _authorPtr = ObsPlugin.StringToUtf8Ptr(author);
        }
        return _authorPtr;
    }
}

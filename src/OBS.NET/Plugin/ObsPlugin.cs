using System.Runtime.InteropServices;
using System.Text;
using OBS.NET.Native;

namespace OBS.NET.Plugin;

/// <summary>
/// Base class for OBS plugins written in C#
/// </summary>
public abstract class ObsPlugin
{
    private static ObsPlugin? _instance;
    private static List<object>? _pinnedDelegates;
    private static List<GCHandle>? _pinnedHandles;

    // Lazy initialization to avoid static constructor issues with NativeAOT
    private static List<object> PinnedDelegates => _pinnedDelegates ??= new List<object>();
    private static List<GCHandle> PinnedHandles => _pinnedHandles ??= new List<GCHandle>();

    /// <summary>
    /// The current plugin instance
    /// </summary>
    public static ObsPlugin? Instance => _instance;

    /// <summary>
    /// Module pointer provided by OBS
    /// </summary>
    public IntPtr Module { get; internal set; }

    /// <summary>
    /// Plugin name
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Plugin author
    /// </summary>
    public virtual string Author => "";

    /// <summary>
    /// Plugin description
    /// </summary>
    public virtual string Description => "";

    /// <summary>
    /// Plugin version
    /// </summary>
    public virtual uint Version => 0x010000; // 1.0.0

    /// <summary>
    /// Called when the module is loaded
    /// </summary>
    public abstract bool Load();

    /// <summary>
    /// Called when the module is unloaded
    /// </summary>
    public virtual void Unload() { }

    /// <summary>
    /// Called after all modules have been loaded
    /// </summary>
    public virtual void PostLoad() { }

    /// <summary>
    /// Initialize the plugin. Called from the native entry point.
    /// </summary>
    protected static void Initialize(ObsPlugin plugin)
    {
        _instance = plugin;
    }

    /// <summary>
    /// Registers a source type with OBS
    /// </summary>
    public void RegisterSource<T>() where T : ObsSource, new()
    {
        var source = new T();
        source.Register();
    }

    /// <summary>
    /// Registers a filter type with OBS
    /// </summary>
    public void RegisterFilter<T>() where T : ObsFilter, new()
    {
        var filter = new T();
        filter.Register();
    }

    /// <summary>
    /// Registers an output type with OBS
    /// </summary>
    public void RegisterOutput<T>() where T : ObsOutput, new()
    {
        var output = new T();
        output.Register();
    }

    /// <summary>
    /// Converts a string to a null-terminated UTF-8 byte array and allocates unmanaged memory
    /// </summary>
    /// <param name="str">The string to convert</param>
    /// <returns>Pointer to the unmanaged memory containing the UTF-8 string</returns>
    public static IntPtr StringToUtf8Ptr(string str)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(str + "\0");
        IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        return ptr;
    }

    /// <summary>
    /// Log a message to OBS
    /// </summary>
    public static void Log(int level, string message)
    {
        var ptr = StringToUtf8Ptr(message);
        try
        {
            ObsApi.Blog(level, ptr);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    /// <summary>
    /// Log info message
    /// </summary>
    public static void LogInfo(string message) => Log(LogLevel.Info, message);

    /// <summary>
    /// Log warning message
    /// </summary>
    public static void LogWarning(string message) => Log(LogLevel.Warning, message);

    /// <summary>
    /// Log error message
    /// </summary>
    public static void LogError(string message) => Log(LogLevel.Error, message);

    /// <summary>
    /// Log debug message
    /// </summary>
    public static void LogDebug(string message) => Log(LogLevel.Debug, message);

    /// <summary>
    /// Pin a delegate to prevent garbage collection
    /// </summary>
    internal static IntPtr PinDelegate<T>(T del) where T : Delegate
    {
        PinnedDelegates.Add(del);
        return Marshal.GetFunctionPointerForDelegate(del);
    }

    /// <summary>
    /// Pin an object and return its GCHandle
    /// </summary>
    internal static GCHandle PinObject(object obj)
    {
        var handle = GCHandle.Alloc(obj, GCHandleType.Normal);
        PinnedHandles.Add(handle);
        return handle;
    }

    /// <summary>
    /// Free all pinned resources
    /// </summary>
    internal static void FreePinnedResources()
    {
        // DO NOT clear _pinnedDelegates - OBS may still need to call registered callbacks
        // during its shutdown sequence (obs_source_destroy_defer, signal_handler_signal, etc.)
        // _pinnedDelegates?.Clear();  // REMOVED - causes crash on shutdown
        if (_pinnedHandles != null)
        {
            foreach (var handle in _pinnedHandles)
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
            _pinnedHandles.Clear();
        }
    }
}

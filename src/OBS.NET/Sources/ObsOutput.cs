using System.Runtime.InteropServices;
using OBS.NET.Native;

namespace OBS.NET.Plugin;

/// <summary>
/// Output flags for OBS outputs
/// </summary>
[Flags]
public enum ObsOutputFlags : uint
{
    Video = (1 << 0),
    Audio = (1 << 1),
    Av = Video | Audio,
    Encoded = (1 << 2),
    Service = (1 << 3),
    Multi = (1 << 4)
}

/// <summary>
/// Base class for OBS output implementations
/// </summary>
public abstract class ObsOutput
{
    private static readonly Dictionary<IntPtr, ObsOutput> _instances = new();

    private ObsOutputContext _outputContext;
    private ObsData _settings;
    private IntPtr _namePtr;

    /// <summary>
    /// Unique identifier for this output type
    /// </summary>
    public abstract string Id { get; }

    /// <summary>
    /// Display name for this output type
    /// </summary>
    public abstract string DisplayName { get; }

    /// <summary>
    /// Output flags
    /// </summary>
    public virtual ObsOutputFlags Flags => ObsOutputFlags.Video;

    /// <summary>
    /// The native output context
    /// </summary>
    protected ObsOutputContext Context => _outputContext;

    /// <summary>
    /// Current settings
    /// </summary>
    protected ObsData Settings => _settings;

    #region Virtual Methods

    /// <summary>
    /// Called when the output is created
    /// </summary>
    protected virtual void OnCreate(ObsData settings) { }

    /// <summary>
    /// Called when the output is destroyed
    /// </summary>
    protected virtual void OnDestroy() { }

    /// <summary>
    /// Called when the output is started
    /// </summary>
    protected virtual bool OnStart() => true;

    /// <summary>
    /// Called when the output is stopped
    /// </summary>
    protected virtual void OnStop(ulong timestamp) { }

    /// <summary>
    /// Called when settings are updated
    /// </summary>
    protected virtual void OnUpdate(ObsData settings) { }

    /// <summary>
    /// Get default settings for this output
    /// </summary>
    protected virtual void GetDefaults(ObsData settings) { }

    /// <summary>
    /// Get properties for this output
    /// </summary>
    protected virtual ObsProperties? GetProperties() => null;

    /// <summary>
    /// Called when a raw video frame is received
    /// </summary>
    protected virtual void OnRawVideo(ref ObsSourceFrame frame) { }

    /// <summary>
    /// Called when raw audio data is received
    /// </summary>
    protected virtual void OnRawAudio(ref ObsSourceAudio audio) { }

    #endregion

    #region Registration

    /// <summary>
    /// Registers this output type with OBS
    /// </summary>
    internal void Register()
    {
        _namePtr = ObsPlugin.StringToUtf8Ptr(DisplayName);
        var idPtr = ObsPlugin.StringToUtf8Ptr(Id);

        var info = new ObsOutputInfo
        {
            Id = idPtr,
            Flags = (uint)Flags,

            GetName = ObsPlugin.PinDelegate<GetNameDelegate>(GetNameCallback),
            Create = ObsPlugin.PinDelegate<OutputCreateDelegate>(CreateCallback),
            Destroy = ObsPlugin.PinDelegate<DestroyDelegate>(DestroyCallback),
            Start = ObsPlugin.PinDelegate<OutputStartDelegate>(StartCallback),
            Stop = ObsPlugin.PinDelegate<OutputStopDelegate>(StopCallback),
            Update = ObsPlugin.PinDelegate<UpdateDelegate>(UpdateCallback),
            GetDefaults = ObsPlugin.PinDelegate<GetDefaultsDelegate>(GetDefaultsCallback),
            GetProperties = ObsPlugin.PinDelegate<GetPropertiesDelegate>(GetPropertiesCallback),
            RawVideo = ObsPlugin.PinDelegate<OutputRawVideoDelegate>(RawVideoCallback),
            RawAudio = ObsPlugin.PinDelegate<OutputRawAudioDelegate>(RawAudioCallback),
        };

        ObsApi.RegisterOutput(ref info);
        ObsPlugin.LogInfo($"Registered output: {Id}");
    }

    #endregion

    #region Native Callbacks

    private IntPtr GetNameCallback(IntPtr typeData)
    {
        return _namePtr;
    }

    private IntPtr CreateCallback(ObsData settings, ObsOutputContext output)
    {
        try
        {
            var instance = (ObsOutput)Activator.CreateInstance(GetType())!;
            instance._outputContext = output;
            instance._settings = settings;

            var handle = ObsPlugin.PinObject(instance);
            var ptr = GCHandle.ToIntPtr(handle);
            _instances[ptr] = instance;

            instance.OnCreate(settings);
            return ptr;
        }
        catch (Exception ex)
        {
            ObsPlugin.LogError($"Error creating output {Id}: {ex.Message}");
            return IntPtr.Zero;
        }
    }

    private static void DestroyCallback(IntPtr data)
    {
        if (_instances.TryGetValue(data, out var instance))
        {
            try
            {
                instance.OnDestroy();
            }
            catch (Exception ex)
            {
                ObsPlugin.LogError($"Error destroying output: {ex.Message}");
            }
            finally
            {
                _instances.Remove(data);
            }
        }
    }

    private static bool StartCallback(IntPtr data)
    {
        if (_instances.TryGetValue(data, out var instance))
        {
            try
            {
                return instance.OnStart();
            }
            catch (Exception ex)
            {
                ObsPlugin.LogError($"Error starting output: {ex.Message}");
            }
        }
        return false;
    }

    private static void StopCallback(IntPtr data, ulong timestamp)
    {
        if (_instances.TryGetValue(data, out var instance))
        {
            try
            {
                instance.OnStop(timestamp);
            }
            catch (Exception ex)
            {
                ObsPlugin.LogError($"Error stopping output: {ex.Message}");
            }
        }
    }

    private void GetDefaultsCallback(ObsData settings)
    {
        try
        {
            GetDefaults(settings);
        }
        catch (Exception ex)
        {
            ObsPlugin.LogError($"Error getting defaults for output {Id}: {ex.Message}");
        }
    }

    private static ObsProperties GetPropertiesCallback(IntPtr data)
    {
        if (_instances.TryGetValue(data, out var instance))
        {
            try
            {
                return instance.GetProperties() ?? ObsProperties.Null;
            }
            catch (Exception ex)
            {
                ObsPlugin.LogError($"Error getting output properties: {ex.Message}");
            }
        }
        return ObsProperties.Null;
    }

    private static void UpdateCallback(IntPtr data, ObsData settings)
    {
        if (_instances.TryGetValue(data, out var instance))
        {
            try
            {
                instance._settings = settings;
                instance.OnUpdate(settings);
            }
            catch (Exception ex)
            {
                ObsPlugin.LogError($"Error updating output: {ex.Message}");
            }
        }
    }

    private static void RawVideoCallback(IntPtr data, ref ObsSourceFrame frame)
    {
        if (_instances.TryGetValue(data, out var instance))
        {
            try
            {
                instance.OnRawVideo(ref frame);
            }
            catch (Exception ex)
            {
                ObsPlugin.LogError($"Error processing raw video: {ex.Message}");
            }
        }
    }

    private static void RawAudioCallback(IntPtr data, ref ObsSourceAudio audio)
    {
        if (_instances.TryGetValue(data, out var instance))
        {
            try
            {
                instance.OnRawAudio(ref audio);
            }
            catch (Exception ex)
            {
                ObsPlugin.LogError($"Error processing raw audio: {ex.Message}");
            }
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Begin capturing data
    /// </summary>
    protected bool BeginDataCapture(uint flags = 0)
    {
        if (!_outputContext.IsValid) return false;
        return ObsApi.OutputBeginDataCapture(_outputContext, flags);
    }

    /// <summary>
    /// End capturing data
    /// </summary>
    protected void EndDataCapture()
    {
        if (_outputContext.IsValid)
        {
            ObsApi.OutputEndDataCapture(_outputContext);
        }
    }

    /// <summary>
    /// Signal that the output has stopped
    /// </summary>
    protected void SignalStop(int code = 0)
    {
        if (_outputContext.IsValid)
        {
            ObsApi.OutputSignalStop(_outputContext, code);
        }
    }

    #endregion
}

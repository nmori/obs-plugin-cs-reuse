using System.Runtime.InteropServices;
using OBS.NET.Native;

namespace OBS.NET.Plugin;

/// <summary>
/// Base class for OBS source implementations
/// </summary>
public abstract class ObsSource
{
    private static readonly Dictionary<IntPtr, ObsSource> _instances = new();

    private ObsSourceContext _sourceContext;
    private ObsData _settings;
    private IntPtr _namePtr;

    /// <summary>
    /// Unique identifier for this source type
    /// </summary>
    public abstract string Id { get; }

    /// <summary>
    /// Display name for this source type
    /// </summary>
    public abstract string DisplayName { get; }

    /// <summary>
    /// Source type (Input, Filter, Transition, Scene)
    /// </summary>
    public virtual ObsSourceType SourceType => ObsSourceType.Input;

    /// <summary>
    /// Output flags for this source
    /// </summary>
    public virtual ObsSourceFlags Flags => ObsSourceFlags.Video;

    /// <summary>
    /// Icon type for this source
    /// </summary>
    public virtual ObsIconType IconType => ObsIconType.Unknown;

    /// <summary>
    /// Version of this source
    /// </summary>
    public virtual uint Version => 1;

    /// <summary>
    /// Width of the source (for non-async sources)
    /// </summary>
    public virtual uint Width => 0;

    /// <summary>
    /// Height of the source (for non-async sources)
    /// </summary>
    public virtual uint Height => 0;

    /// <summary>
    /// The native source context
    /// </summary>
    protected ObsSourceContext Context => _sourceContext;

    /// <summary>
    /// Current settings
    /// </summary>
    protected ObsData Settings => _settings;

    #region Virtual Methods

    /// <summary>
    /// Called when the source is created
    /// </summary>
    protected virtual void OnCreate(ObsData settings) { }

    /// <summary>
    /// Called when the source is destroyed
    /// </summary>
    protected virtual void OnDestroy() { }

    /// <summary>
    /// Get default settings for this source
    /// </summary>
    protected virtual void GetDefaults(ObsData settings) { }

    /// <summary>
    /// Get properties for this source
    /// </summary>
    protected virtual ObsProperties? GetProperties() => null;

    /// <summary>
    /// Called when settings are updated
    /// </summary>
    protected virtual void OnUpdate(ObsData settings) { }

    /// <summary>
    /// Called when the source becomes active
    /// </summary>
    protected virtual void OnActivate() { }

    /// <summary>
    /// Called when the source becomes inactive
    /// </summary>
    protected virtual void OnDeactivate() { }

    /// <summary>
    /// Called when the source is shown
    /// </summary>
    protected virtual void OnShow() { }

    /// <summary>
    /// Called when the source is hidden
    /// </summary>
    protected virtual void OnHide() { }

    /// <summary>
    /// Called every frame tick
    /// </summary>
    protected virtual void OnVideoTick(float seconds) { }

    /// <summary>
    /// Called to render video
    /// </summary>
    protected virtual void OnVideoRender(GsEffect effect) { }

    /// <summary>
    /// Called when saving the source
    /// </summary>
    protected virtual void OnSave(ObsData settings) { }

    /// <summary>
    /// Called when loading the source
    /// </summary>
    protected virtual void OnLoad(ObsData settings) { }

    #endregion

    #region Registration

    /// <summary>
    /// Registers this source type with OBS
    /// </summary>
    internal void Register()
    {
        _namePtr = ObsPlugin.StringToUtf8Ptr(DisplayName);
        var idPtr = ObsPlugin.StringToUtf8Ptr(Id);

        var info = new ObsSourceInfo
        {
            Id = idPtr,
            Type = SourceType,
            OutputFlags = Flags,
            IconType = IconType,
            Version = Version,

            GetName = ObsPlugin.PinDelegate<GetNameDelegate>(GetNameCallback),
            Create = ObsPlugin.PinDelegate<CreateDelegate>(CreateCallback),
            Destroy = ObsPlugin.PinDelegate<DestroyDelegate>(DestroyCallback),
            GetWidth = ObsPlugin.PinDelegate<GetWidthDelegate>(GetWidthCallback),
            GetHeight = ObsPlugin.PinDelegate<GetHeightDelegate>(GetHeightCallback),
            GetDefaults = ObsPlugin.PinDelegate<GetDefaultsDelegate>(GetDefaultsCallback),
            GetProperties = ObsPlugin.PinDelegate<GetPropertiesDelegate>(GetPropertiesCallback),
            Update = ObsPlugin.PinDelegate<UpdateDelegate>(UpdateCallback),
            Activate = ObsPlugin.PinDelegate<ActivateDelegate>(ActivateCallback),
            Deactivate = ObsPlugin.PinDelegate<DeactivateDelegate>(DeactivateCallback),
            Show = ObsPlugin.PinDelegate<ShowDelegate>(ShowCallback),
            Hide = ObsPlugin.PinDelegate<HideDelegate>(HideCallback),
            VideoTick = ObsPlugin.PinDelegate<VideoTickDelegate>(VideoTickCallback),
            VideoRender = ObsPlugin.PinDelegate<VideoRenderDelegate>(VideoRenderCallback),
            Save = ObsPlugin.PinDelegate<SaveDelegate>(SaveCallback),
            Load = ObsPlugin.PinDelegate<LoadDelegate>(LoadCallback),
        };

        // Add FilterAudio for audio filters
        if (this is ObsAudioFilter)
        {
            info.FilterAudio = ObsPlugin.PinDelegate<FilterAudioDelegate>(FilterAudioStaticCallback);
        }

        ObsApi.RegisterSource(ref info);
        ObsPlugin.LogInfo($"Registered source: {Id}");
    }

    /// <summary>
    /// Static callback for filter_audio
    /// </summary>
    private static IntPtr FilterAudioStaticCallback(IntPtr data, IntPtr audioDataPtr)
    {
        if (_instances.TryGetValue(data, out var instance) && instance is ObsAudioFilter audioFilter)
        {
            try
            {
                return audioFilter.InvokeFilterAudio(audioDataPtr);
            }
            catch (Exception ex)
            {
                ObsPlugin.LogError($"Error in audio filter: {ex.Message}");
            }
        }
        return audioDataPtr; // Pass through on error
    }

    #endregion

    #region Native Callbacks

    private IntPtr GetNameCallback(IntPtr typeData)
    {
        return _namePtr;
    }

    private IntPtr CreateCallback(ObsData settings, ObsSourceContext source)
    {
        try
        {
            var instance = (ObsSource)Activator.CreateInstance(GetType())!;
            instance._sourceContext = source;
            instance._settings = settings;

            var handle = ObsPlugin.PinObject(instance);
            var ptr = GCHandle.ToIntPtr(handle);
            _instances[ptr] = instance;

            instance.OnCreate(settings);
            return ptr;
        }
        catch (Exception ex)
        {
            ObsPlugin.LogError($"Error creating source {Id}: {ex.Message}");
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
                ObsPlugin.LogError($"Error destroying source: {ex.Message}");
            }
            finally
            {
                _instances.Remove(data);
            }
        }
    }

    private static uint GetWidthCallback(IntPtr data)
    {
        if (_instances.TryGetValue(data, out var instance))
        {
            return instance.Width;
        }
        return 0;
    }

    private static uint GetHeightCallback(IntPtr data)
    {
        if (_instances.TryGetValue(data, out var instance))
        {
            return instance.Height;
        }
        return 0;
    }

    private void GetDefaultsCallback(ObsData settings)
    {
        try
        {
            GetDefaults(settings);
        }
        catch (Exception ex)
        {
            ObsPlugin.LogError($"Error getting defaults for {Id}: {ex.Message}");
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
                ObsPlugin.LogError($"Error getting properties: {ex.Message}");
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
                ObsPlugin.LogError($"Error updating source: {ex.Message}");
            }
        }
    }

    private static void ActivateCallback(IntPtr data)
    {
        if (_instances.TryGetValue(data, out var instance))
        {
            try
            {
                instance.OnActivate();
            }
            catch (Exception ex)
            {
                ObsPlugin.LogError($"Error activating source: {ex.Message}");
            }
        }
    }

    private static void DeactivateCallback(IntPtr data)
    {
        if (_instances.TryGetValue(data, out var instance))
        {
            try
            {
                instance.OnDeactivate();
            }
            catch (Exception ex)
            {
                ObsPlugin.LogError($"Error deactivating source: {ex.Message}");
            }
        }
    }

    private static void ShowCallback(IntPtr data)
    {
        if (_instances.TryGetValue(data, out var instance))
        {
            try
            {
                instance.OnShow();
            }
            catch (Exception ex)
            {
                ObsPlugin.LogError($"Error showing source: {ex.Message}");
            }
        }
    }

    private static void HideCallback(IntPtr data)
    {
        if (_instances.TryGetValue(data, out var instance))
        {
            try
            {
                instance.OnHide();
            }
            catch (Exception ex)
            {
                ObsPlugin.LogError($"Error hiding source: {ex.Message}");
            }
        }
    }

    private static void VideoTickCallback(IntPtr data, float seconds)
    {
        if (_instances.TryGetValue(data, out var instance))
        {
            try
            {
                instance.OnVideoTick(seconds);
            }
            catch (Exception ex)
            {
                ObsPlugin.LogError($"Error in video tick: {ex.Message}");
            }
        }
    }

    private static void VideoRenderCallback(IntPtr data, GsEffect effect)
    {
        if (_instances.TryGetValue(data, out var instance))
        {
            try
            {
                instance.OnVideoRender(effect);
            }
            catch (Exception ex)
            {
                ObsPlugin.LogError($"Error in video render: {ex.Message}");
            }
        }
    }

    private static void SaveCallback(IntPtr data, ObsData settings)
    {
        if (_instances.TryGetValue(data, out var instance))
        {
            try
            {
                instance.OnSave(settings);
            }
            catch (Exception ex)
            {
                ObsPlugin.LogError($"Error saving source: {ex.Message}");
            }
        }
    }

    private static void LoadCallback(IntPtr data, ObsData settings)
    {
        if (_instances.TryGetValue(data, out var instance))
        {
            try
            {
                instance.OnLoad(settings);
            }
            catch (Exception ex)
            {
                ObsPlugin.LogError($"Error loading source: {ex.Message}");
            }
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get an instance of the specified type by data pointer
    /// Used by subclasses for static callback lookups
    /// </summary>
    protected static T? GetInstance<T>(IntPtr data) where T : ObsSource
    {
        if (_instances.TryGetValue(data, out var instance))
        {
            return instance as T;
        }
        return null;
    }

    /// <summary>
    /// Output a video frame (for async video sources)
    /// </summary>
    protected void OutputVideo(ref ObsSourceFrame frame)
    {
        if (_sourceContext.IsValid)
        {
            ObsApi.SourceOutputVideo(_sourceContext, ref frame);
        }
    }

    /// <summary>
    /// Output audio data
    /// </summary>
    protected void OutputAudio(ref ObsSourceAudio audio)
    {
        if (_sourceContext.IsValid)
        {
            ObsApi.SourceOutputAudio(_sourceContext, ref audio);
        }
    }

    #endregion
}

using System.Runtime.InteropServices;
using OBS.NET.Native;

namespace OBS.NET.Plugin;

/// <summary>
/// Base class for OBS filter implementations
/// </summary>
public abstract class ObsFilter : ObsSource
{
    /// <summary>
    /// Source type is always Filter for filters
    /// </summary>
    public override ObsSourceType SourceType => ObsSourceType.Filter;

    /// <summary>
    /// Gets the parent source this filter is attached to
    /// </summary>
    protected ObsSourceContext ParentSource => Context.IsValid ? ObsApi.FilterGetParent(Context) : ObsSourceContext.Null;

    /// <summary>
    /// Gets the target source (the source being filtered)
    /// </summary>
    protected ObsSourceContext TargetSource => Context.IsValid ? ObsApi.FilterGetTarget(Context) : ObsSourceContext.Null;

    /// <summary>
    /// Gets whether this filter is enabled (checkbox in filter panel)
    /// </summary>
    protected bool IsEnabled => Context.IsValid && ObsApi.SourceEnabled(Context);

    /// <summary>
    /// Width of the filter (defaults to parent width)
    /// </summary>
    public override uint Width
    {
        get
        {
            var target = TargetSource;
            return target.IsValid ? ObsApi.SourceGetWidth(target) : 0;
        }
    }

    /// <summary>
    /// Height of the filter (defaults to parent height)
    /// </summary>
    public override uint Height
    {
        get
        {
            var target = TargetSource;
            return target.IsValid ? ObsApi.SourceGetHeight(target) : 0;
        }
    }

    /// <summary>
    /// Skip this filter's video processing (pass through)
    /// </summary>
    protected void SkipVideoFilter()
    {
        if (Context.IsValid)
        {
            ObsApi.SourceSkipVideoFilter(Context);
        }
    }

    /// <summary>
    /// Begin filter processing
    /// </summary>
    protected bool BeginFilterProcess(int format = 0, bool allowDirect = true)
    {
        if (!Context.IsValid) return false;
        return ObsApi.SourceProcessFilterBegin(Context, format, allowDirect ? 1 : 0);
    }

    /// <summary>
    /// End filter processing
    /// </summary>
    protected void EndFilterProcess(GsEffect effect)
    {
        if (Context.IsValid)
        {
            ObsApi.SourceProcessFilterEnd(Context, effect, Width, Height);
        }
    }
}

/// <summary>
/// Base class for video filters
/// </summary>
public abstract class ObsVideoFilter : ObsFilter
{
    public override ObsSourceFlags Flags => ObsSourceFlags.Video;
}

/// <summary>
/// Base class for audio filters
/// </summary>
public abstract class ObsAudioFilter : ObsFilter
{
    public override ObsSourceFlags Flags => ObsSourceFlags.Audio;

    /// <summary>
    /// Audio filters don't have width (prevents recursive TargetSource access)
    /// </summary>
    public override uint Width => 0;

    /// <summary>
    /// Audio filters don't have height (prevents recursive TargetSource access)
    /// </summary>
    public override uint Height => 0;

    /// <summary>
    /// Called to filter audio data. Return the modified audio data pointer.
    /// The audio data is passed through by default (no modification).
    /// </summary>
    protected virtual IntPtr OnFilterAudio(IntPtr audioDataPtr)
    {
        // Pass through by default
        return audioDataPtr;
    }

    /// <summary>
    /// Internal wrapper to call OnFilterAudio from ObsSource
    /// </summary>
    internal IntPtr InvokeFilterAudio(IntPtr audioDataPtr)
    {
        return OnFilterAudio(audioDataPtr);
    }
}

/// <summary>
/// Base class for async video filters (for modifying video frames)
/// </summary>
public abstract class ObsAsyncVideoFilter : ObsFilter
{
    public override ObsSourceFlags Flags => ObsSourceFlags.AsyncVideo;

    /// <summary>
    /// Called to filter video frames
    /// </summary>
    protected virtual ObsSourceFrame? OnFilterVideo(ref ObsSourceFrame frame) => frame;
}

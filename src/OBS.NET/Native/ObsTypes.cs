using System.Runtime.InteropServices;

namespace OBS.NET.Native;

public enum ObsSourceType : int
{
    Input = 0,
    Filter = 1,
    Transition = 2,
    Scene = 3
}

[Flags]
public enum ObsSourceFlags : uint
{
    None = 0,
    Video = (1 << 0),
    Audio = (1 << 1),
    Async = (1 << 2),
    AsyncVideo = Async | Video,
    CustomDraw = (1 << 3),
    Interaction = (1 << 5),
    Composite = (1 << 6),
    DoNotDuplicate = (1 << 7),
    Deprecated = (1 << 8),
    DoNotSelfMonitor = (1 << 9),
    Submix = (1 << 10),
    Controllable = (1 << 11),
    CatDontShow = (1 << 12),
    Srgb = (1 << 13),
}

public enum ObsIconType : int
{
    Unknown = 0, Image, Color, Slideshow, AudioInput, AudioOutput,
    DesktopCapture, WindowCapture, GameCapture, Camera, Text, Media,
    Browser, Custom, ProcessAudioOutput, Scene
}

public enum VideoFormat : int
{
    None = 0, I420, NV12, YVYU, YUY2, UYVY, RGBA, BGRA, BGRX, Y800,
    I444, BGR3, I422, I40A, I42A, YUVA, AYUV, I010, P010, I210, I412,
    YA2L, P216, P416, V210
}

public enum AudioFormat : int
{
    Unknown = 0, U8Bit, S16Bit, S32Bit, Float,
    U8BitPlanar, S16BitPlanar, S32BitPlanar, FloatPlanar
}

public enum SpeakerLayout : int
{
    Unknown = 0, Mono, Stereo, Layout2Point1, Layout4Point0,
    Layout4Point1, Layout5Point1, Layout7Point1 = 8
}

[StructLayout(LayoutKind.Sequential)]
public struct ObsData
{
    public IntPtr Pointer;
    public bool IsValid => Pointer != IntPtr.Zero;
    public static ObsData Null => new() { Pointer = IntPtr.Zero };
}

[StructLayout(LayoutKind.Sequential)]
public struct ObsSourceContext
{
    public IntPtr Pointer;
    public bool IsValid => Pointer != IntPtr.Zero;
    public static ObsSourceContext Null => new() { Pointer = IntPtr.Zero };
}

[StructLayout(LayoutKind.Sequential)]
public struct ObsOutputContext
{
    public IntPtr Pointer;
    public bool IsValid => Pointer != IntPtr.Zero;
    public static ObsOutputContext Null => new() { Pointer = IntPtr.Zero };
}

[StructLayout(LayoutKind.Sequential)]
public struct ObsProperties
{
    public IntPtr Pointer;
    public bool IsValid => Pointer != IntPtr.Zero;
    public static ObsProperties Null => new() { Pointer = IntPtr.Zero };
}

[StructLayout(LayoutKind.Sequential)]
public struct ObsProperty
{
    public IntPtr Pointer;
    public bool IsValid => Pointer != IntPtr.Zero;
    public static ObsProperty Null => new() { Pointer = IntPtr.Zero };
}

[StructLayout(LayoutKind.Sequential)]
public struct DataPointers
{
    public IntPtr Data0, Data1, Data2, Data3, Data4, Data5, Data6, Data7;

    public IntPtr this[int index]
    {
        get => index switch
        {
            0 => Data0, 1 => Data1, 2 => Data2, 3 => Data3,
            4 => Data4, 5 => Data5, 6 => Data6, 7 => Data7,
            _ => throw new IndexOutOfRangeException()
        };
        set { switch (index) {
            case 0: Data0 = value; break; case 1: Data1 = value; break;
            case 2: Data2 = value; break; case 3: Data3 = value; break;
            case 4: Data4 = value; break; case 5: Data5 = value; break;
            case 6: Data6 = value; break; case 7: Data7 = value; break;
            default: throw new IndexOutOfRangeException();
        }}
    }
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ObsSourceFrame
{
    public DataPointers Data;
    public fixed uint Linesize[8];
    public uint Width, Height;
    public ulong Timestamp;
    public VideoFormat Format;
    public fixed float ColorMatrix[16];
    public byte FullRange;
    public fixed float ColorRangeMin[3];
    public fixed float ColorRangeMax[3];
    public byte Flip, TrcOverride, PrimOverride;

    public static ObsSourceFrame Create(uint width, uint height, VideoFormat format)
    {
        return new ObsSourceFrame { Width = width, Height = height, Format = format };
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct ObsSourceAudio
{
    public DataPointers Data;
    public uint Frames;
    public SpeakerLayout Speakers;
    public AudioFormat Format;
    public uint SamplesPerSec;
    public ulong Timestamp;
}

/// <summary>
/// obs_audio_info structure for global audio settings
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ObsAudioInfo
{
    public uint SamplesPerSec;
    public SpeakerLayout Speakers;
}

/// <summary>
/// obs_audio_data structure for filter_audio callback
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ObsAudioData
{
    public DataPointers Data;
    public uint Frames;
    public ulong Timestamp;
}

[StructLayout(LayoutKind.Sequential)]
public struct GsEffect
{
    public IntPtr Pointer;
    public bool IsValid => Pointer != IntPtr.Zero;
    public static GsEffect Null => new() { Pointer = IntPtr.Zero };
}

[StructLayout(LayoutKind.Sequential)]
public struct GsTexture
{
    public IntPtr Pointer;
    public bool IsValid => Pointer != IntPtr.Zero;
    public static GsTexture Null => new() { Pointer = IntPtr.Zero };
}

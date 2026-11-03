using System.Runtime.InteropServices;

namespace OBS.NET.Native;

/// <summary>
/// Delegate definitions for OBS source callbacks
/// </summary>

// Module callbacks
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool ObsModuleLoadDelegate();

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void ObsModuleUnloadDelegate();

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void ObsModulePostLoadDelegate();

// Source info callbacks
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr GetNameDelegate(IntPtr typeData);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr CreateDelegate(ObsData settings, ObsSourceContext source);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void DestroyDelegate(IntPtr data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate uint GetWidthDelegate(IntPtr data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate uint GetHeightDelegate(IntPtr data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void GetDefaultsDelegate(ObsData settings);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate ObsProperties GetPropertiesDelegate(IntPtr data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void UpdateDelegate(IntPtr data, ObsData settings);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void ActivateDelegate(IntPtr data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void DeactivateDelegate(IntPtr data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void ShowDelegate(IntPtr data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HideDelegate(IntPtr data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void VideoTickDelegate(IntPtr data, float seconds);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void VideoRenderDelegate(IntPtr data, GsEffect effect);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr FilterVideoDelegate(IntPtr data, ref ObsSourceFrame frame);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr FilterAudioDelegate(IntPtr data, IntPtr audioDataPtr);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void EnumActiveSourcesDelegate(IntPtr data, IntPtr enumCallback, IntPtr param);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void SaveDelegate(IntPtr data, ObsData settings);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void LoadDelegate(IntPtr data, ObsData settings);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void MouseClickDelegate(IntPtr data, IntPtr mouseEvent, int type, [MarshalAs(UnmanagedType.I1)] bool mouseUp, uint clickCount);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void MouseMoveDelegate(IntPtr data, IntPtr mouseEvent, [MarshalAs(UnmanagedType.I1)] bool mouseLeave);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void MouseWheelDelegate(IntPtr data, IntPtr mouseEvent, int xDelta, int yDelta);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void FocusDelegate(IntPtr data, [MarshalAs(UnmanagedType.I1)] bool focus);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void KeyClickDelegate(IntPtr data, IntPtr keyEvent, [MarshalAs(UnmanagedType.I1)] bool keyUp);

// Filter specific callbacks
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void FilterRemoveDelegate(IntPtr data, ObsSourceContext source);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void FilterAddDelegate(IntPtr data, ObsSourceContext source);

// Output callbacks
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr OutputCreateDelegate(ObsData settings, ObsOutputContext output);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.I1)]
public delegate bool OutputStartDelegate(IntPtr data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void OutputStopDelegate(IntPtr data, ulong ts);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void OutputRawVideoDelegate(IntPtr data, ref ObsSourceFrame frame);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void OutputRawAudioDelegate(IntPtr data, ref ObsSourceAudio audio);

// Enumeration callbacks
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.I1)]
public delegate bool EnumSourceCallback(IntPtr param, ObsSourceContext source);

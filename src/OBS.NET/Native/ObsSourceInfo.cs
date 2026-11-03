using System.Runtime.InteropServices;

namespace OBS.NET.Native;

/// <summary>
/// obs_source_info structure for registering sources with OBS
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ObsSourceInfo
{
    // Required fields
    public IntPtr Id;                          // const char *id
    public ObsSourceType Type;                 // enum obs_source_type
    public ObsSourceFlags OutputFlags;         // uint32_t output_flags

    // Name callback
    public IntPtr GetName;                     // const char *(*get_name)(void *type_data)

    // Creation/destruction
    public IntPtr Create;                      // void *(*create)(obs_data_t *settings, obs_source_t *source)
    public IntPtr Destroy;                     // void (*destroy)(void *data)

    // Size info (for non-async video sources)
    public IntPtr GetWidth;                    // uint32_t (*get_width)(void *data)
    public IntPtr GetHeight;                   // uint32_t (*get_height)(void *data)

    // Settings
    public IntPtr GetDefaults;                 // void (*get_defaults)(obs_data_t *settings)
    public IntPtr GetProperties;               // obs_properties_t *(*get_properties)(void *data)
    public IntPtr Update;                      // void (*update)(void *data, obs_data_t *settings)

    // Activation/visibility
    public IntPtr Activate;                    // void (*activate)(void *data)
    public IntPtr Deactivate;                  // void (*deactivate)(void *data)
    public IntPtr Show;                        // void (*show)(void *data)
    public IntPtr Hide;                        // void (*hide)(void *data)

    // Video
    public IntPtr VideoTick;                   // void (*video_tick)(void *data, float seconds)
    public IntPtr VideoRender;                 // void (*video_render)(void *data, gs_effect_t *effect)

    // Filter callbacks
    public IntPtr FilterVideo;                 // struct obs_source_frame *(*filter_video)(void *data, struct obs_source_frame *frame)
    public IntPtr FilterAudio;                 // struct obs_audio_data *(*filter_audio)(void *data, struct obs_audio_data *audio)

    // Enumeration
    public IntPtr EnumActiveSources;           // void (*enum_active_sources)(void *data, obs_source_enum_proc_t enum_callback, void *param)

    // Save/load
    public IntPtr Save;                        // void (*save)(void *data, obs_data_t *settings)
    public IntPtr Load;                        // void (*load)(void *data, obs_data_t *settings)

    // Mouse/keyboard
    public IntPtr MouseClick;                  // void (*mouse_click)(void *data, ...)
    public IntPtr MouseMove;                   // void (*mouse_move)(void *data, ...)
    public IntPtr MouseWheel;                  // void (*mouse_wheel)(void *data, ...)
    public IntPtr Focus;                       // void (*focus)(void *data, bool focus)
    public IntPtr KeyClick;                    // void (*key_click)(void *data, ...)

    // Filter lifecycle
    public IntPtr FilterRemove;                // void (*filter_remove)(void *data, obs_source_t *source)
    public IntPtr FilterAdd;                   // void (*filter_add)(void *data, obs_source_t *source)

    // Type data
    public IntPtr TypeData;                    // void *type_data
    public IntPtr FreeTypeData;                // void (*free_type_data)(void *type_data)

    // Audio mixing
    public IntPtr AudioMix;                    // bool (*audio_mix)(void *data, ...)

    // Media control
    public IntPtr MediaPlayPause;              // void (*media_play_pause)(void *data, bool pause)
    public IntPtr MediaRestart;                // void (*media_restart)(void *data)
    public IntPtr MediaStop;                   // void (*media_stop)(void *data)
    public IntPtr MediaNext;                   // void (*media_next)(void *data)
    public IntPtr MediaPrevious;               // void (*media_previous)(void *data)
    public IntPtr MediaGetDuration;            // int64_t (*media_get_duration)(void *data)
    public IntPtr MediaGetTime;                // int64_t (*media_get_time)(void *data)
    public IntPtr MediaSetTime;                // void (*media_set_time)(void *data, int64_t ms)
    public IntPtr MediaGetState;               // enum obs_media_state (*media_get_state)(void *data)

    // Versioning
    public uint Version;                       // uint32_t version

    // Icon
    public ObsIconType IconType;               // enum obs_icon_type icon_type

    // Additional callbacks (extended API)
    public IntPtr TransitionStart;             // void (*transition_start)(void *data)
    public IntPtr TransitionStop;              // void (*transition_stop)(void *data)

    public IntPtr GetDefaultsEx;               // void (*get_defaults2)(obs_data_t *settings, void *type_data)
    public IntPtr GetPropertiesEx;             // obs_properties_t *(*get_properties2)(void *data, void *type_data)

    // Audio rendering
    public IntPtr AudioRender;                 // bool (*audio_render)(void *data, ...)

    // Video color space
    public IntPtr VideoGetColorSpace;          // enum video_colorspace (*video_get_color_space)(void *data, ...)
}

/// <summary>
/// obs_output_info structure for registering outputs with OBS
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ObsOutputInfo
{
    public IntPtr Id;
    public uint Flags;
    public IntPtr GetName;
    public IntPtr Create;
    public IntPtr Destroy;
    public IntPtr Start;
    public IntPtr Stop;
    public IntPtr RawVideo;
    public IntPtr RawAudio;
    public IntPtr EncodedPacket;
    public IntPtr Update;
    public IntPtr GetDefaults;
    public IntPtr GetProperties;
    public IntPtr Unused;
    public IntPtr GetTotalBytes;
    public IntPtr GetDroppedFrames;
    public IntPtr TypeData;
    public IntPtr FreeTypeData;
    public IntPtr GetConnectTimeMs;
    public IntPtr EncodedVideoFull;
    public IntPtr EncodedAudioFull;
    public uint Version;
    public IntPtr Protocols;
}

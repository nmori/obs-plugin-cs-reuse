using System.Runtime.InteropServices;

namespace OBS.NET.Native;

/// <summary>
/// P/Invoke bindings for OBS Studio native API
/// </summary>
public static partial class ObsApi
{
    private const string ObsLib = "obs";

    #region Module API

    [LibraryImport(ObsLib, EntryPoint = "obs_register_source_s")]
    public static partial void RegisterSource(ref ObsSourceInfo info, nuint size);

    [LibraryImport(ObsLib, EntryPoint = "obs_register_output_s")]
    public static partial void RegisterOutput(ref ObsOutputInfo info, nuint size);

    public static void RegisterSource(ref ObsSourceInfo info)
    {
        RegisterSource(ref info, (nuint)Marshal.SizeOf<ObsSourceInfo>());
    }

    public static void RegisterOutput(ref ObsOutputInfo info)
    {
        RegisterOutput(ref info, (nuint)Marshal.SizeOf<ObsOutputInfo>());
    }

    #endregion

    #region Data API

    [LibraryImport(ObsLib, EntryPoint = "obs_data_create")]
    public static partial ObsData DataCreate();

    [LibraryImport(ObsLib, EntryPoint = "obs_data_release")]
    public static partial void DataRelease(ObsData data);

    [LibraryImport(ObsLib, EntryPoint = "obs_data_get_string", StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr DataGetString(ObsData data, string name);

    [LibraryImport(ObsLib, EntryPoint = "obs_data_set_string", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void DataSetString(ObsData data, string name, string val);

    [LibraryImport(ObsLib, EntryPoint = "obs_data_get_int", StringMarshalling = StringMarshalling.Utf8)]
    public static partial long DataGetInt(ObsData data, string name);

    [LibraryImport(ObsLib, EntryPoint = "obs_data_set_int", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void DataSetInt(ObsData data, string name, long val);

    [LibraryImport(ObsLib, EntryPoint = "obs_data_get_double", StringMarshalling = StringMarshalling.Utf8)]
    public static partial double DataGetDouble(ObsData data, string name);

    [LibraryImport(ObsLib, EntryPoint = "obs_data_set_double", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void DataSetDouble(ObsData data, string name, double val);

    [LibraryImport(ObsLib, EntryPoint = "obs_data_get_bool", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool DataGetBool(ObsData data, string name);

    [LibraryImport(ObsLib, EntryPoint = "obs_data_set_bool", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void DataSetBool(ObsData data, string name, [MarshalAs(UnmanagedType.I1)] bool val);

    [LibraryImport(ObsLib, EntryPoint = "obs_data_set_default_string", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void DataSetDefaultString(ObsData data, string name, string val);

    [LibraryImport(ObsLib, EntryPoint = "obs_data_set_default_int", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void DataSetDefaultInt(ObsData data, string name, long val);

    [LibraryImport(ObsLib, EntryPoint = "obs_data_set_default_double", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void DataSetDefaultDouble(ObsData data, string name, double val);

    [LibraryImport(ObsLib, EntryPoint = "obs_data_set_default_bool", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void DataSetDefaultBool(ObsData data, string name, [MarshalAs(UnmanagedType.I1)] bool val);

    [LibraryImport(ObsLib, EntryPoint = "obs_data_set_obj", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void DataSetObj(ObsData data, string name, ObsData obj);

    [LibraryImport(ObsLib, EntryPoint = "obs_data_get_obj", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ObsData DataGetObj(ObsData data, string name);

    #endregion

    #region Properties API

    [LibraryImport(ObsLib, EntryPoint = "obs_properties_create")]
    public static partial ObsProperties PropertiesCreate();

    [LibraryImport(ObsLib, EntryPoint = "obs_properties_destroy")]
    public static partial void PropertiesDestroy(ObsProperties props);

    [LibraryImport(ObsLib, EntryPoint = "obs_properties_add_bool", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ObsProperty PropertiesAddBool(ObsProperties props, string name, string description);

    [LibraryImport(ObsLib, EntryPoint = "obs_properties_add_int", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ObsProperty PropertiesAddInt(ObsProperties props, string name, string description, int min, int max, int step);

    [LibraryImport(ObsLib, EntryPoint = "obs_properties_add_float", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ObsProperty PropertiesAddFloat(ObsProperties props, string name, string description, double min, double max, double step);

    [LibraryImport(ObsLib, EntryPoint = "obs_properties_add_text", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ObsProperty PropertiesAddText(ObsProperties props, string name, string description, int type);

    [LibraryImport(ObsLib, EntryPoint = "obs_properties_add_path", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ObsProperty PropertiesAddPath(ObsProperties props, string name, string description, int type, string filter, string defaultPath);

    [LibraryImport(ObsLib, EntryPoint = "obs_properties_add_list", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ObsProperty PropertiesAddList(ObsProperties props, string name, string description, int type, int format);

    [LibraryImport(ObsLib, EntryPoint = "obs_properties_add_color", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ObsProperty PropertiesAddColor(ObsProperties props, string name, string description);

    [LibraryImport(ObsLib, EntryPoint = "obs_properties_add_font", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ObsProperty PropertiesAddFont(ObsProperties props, string name, string description);

    [LibraryImport(ObsLib, EntryPoint = "obs_properties_add_button", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ObsProperty PropertiesAddButton(ObsProperties props, string name, string text, IntPtr callback);

    [LibraryImport(ObsLib, EntryPoint = "obs_property_list_add_string", StringMarshalling = StringMarshalling.Utf8)]
    public static partial nuint PropertyListAddString(ObsProperty prop, string name, string val);

    [LibraryImport(ObsLib, EntryPoint = "obs_property_list_add_int", StringMarshalling = StringMarshalling.Utf8)]
    public static partial nuint PropertyListAddInt(ObsProperty prop, string name, long val);

    [LibraryImport(ObsLib, EntryPoint = "obs_properties_get", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ObsProperty PropertiesGet(ObsProperties props, string name);

    [LibraryImport(ObsLib, EntryPoint = "obs_property_set_visible")]
    public static partial void PropertySetVisible(ObsProperty prop, [MarshalAs(UnmanagedType.I1)] bool visible);

    [LibraryImport(ObsLib, EntryPoint = "obs_property_set_modified_callback")]
    public static partial void PropertySetModifiedCallback(ObsProperty prop, IntPtr callback);

    [LibraryImport(ObsLib, EntryPoint = "obs_properties_add_group", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ObsProperty PropertiesAddGroup(ObsProperties props, string name, string description, int type, ObsProperties group);

    [LibraryImport(ObsLib, EntryPoint = "obs_property_group_content")]
    public static partial ObsProperties PropertyGroupContent(ObsProperty prop);

    #endregion

    #region Source API

    [LibraryImport(ObsLib, EntryPoint = "obs_source_create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ObsSourceContext SourceCreate(string id, string name, ObsData settings, IntPtr hotkeys);

    [LibraryImport(ObsLib, EntryPoint = "obs_source_update")]
    public static partial void SourceUpdate(ObsSourceContext source, ObsData settings);

    [LibraryImport(ObsLib, EntryPoint = "obs_source_video_render")]
    public static partial void SourceVideoRender(ObsSourceContext source);

    [LibraryImport(ObsLib, EntryPoint = "obs_source_get_name")]
    public static partial IntPtr SourceGetName(ObsSourceContext source);

    [LibraryImport(ObsLib, EntryPoint = "obs_source_get_uuid")]
    public static partial IntPtr SourceGetUuid(ObsSourceContext source);

    [LibraryImport(ObsLib, EntryPoint = "obs_source_get_width")]
    public static partial uint SourceGetWidth(ObsSourceContext source);

    [LibraryImport(ObsLib, EntryPoint = "obs_source_get_height")]
    public static partial uint SourceGetHeight(ObsSourceContext source);

    [LibraryImport(ObsLib, EntryPoint = "obs_source_output_video")]
    public static partial void SourceOutputVideo(ObsSourceContext source, ref ObsSourceFrame frame);

    [LibraryImport(ObsLib, EntryPoint = "obs_source_output_audio")]
    public static partial void SourceOutputAudio(ObsSourceContext source, ref ObsSourceAudio audio);

    [LibraryImport(ObsLib, EntryPoint = "obs_source_draw")]
    public static partial void SourceDraw(GsTexture texture, int x, int y, uint cx, uint cy, [MarshalAs(UnmanagedType.I1)] bool flip);

    [LibraryImport(ObsLib, EntryPoint = "obs_source_skip_video_filter")]
    public static partial void SourceSkipVideoFilter(ObsSourceContext filter);

    [LibraryImport(ObsLib, EntryPoint = "obs_source_process_filter_begin")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SourceProcessFilterBegin(ObsSourceContext filter, int format, int allowDirect);

    [LibraryImport(ObsLib, EntryPoint = "obs_source_process_filter_end")]
    public static partial void SourceProcessFilterEnd(ObsSourceContext filter, GsEffect effect, uint width, uint height);

    [LibraryImport(ObsLib, EntryPoint = "obs_filter_get_parent")]
    public static partial ObsSourceContext FilterGetParent(ObsSourceContext filter);

    [LibraryImport(ObsLib, EntryPoint = "obs_filter_get_target")]
    public static partial ObsSourceContext FilterGetTarget(ObsSourceContext filter);

    [LibraryImport(ObsLib, EntryPoint = "obs_source_enabled")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SourceEnabled(ObsSourceContext source);

    // SourceGetName は既に Source API セクションに定義済み

    [LibraryImport(ObsLib, EntryPoint = "obs_source_active")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SourceActive(ObsSourceContext source);

    [LibraryImport(ObsLib, EntryPoint = "obs_source_muted")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SourceMuted(ObsSourceContext source);

    #endregion

    #region Scene/SceneItem API

    /// <summary>
    /// ソースからシーンを取得（ソースがシーンの場合）
    /// </summary>
    [LibraryImport(ObsLib, EntryPoint = "obs_scene_from_source")]
    public static partial IntPtr SceneFromSource(ObsSourceContext source);

    /// <summary>
    /// シーン内でソース名からシーンアイテムを検索（再帰的）
    /// </summary>
    [LibraryImport(ObsLib, EntryPoint = "obs_scene_find_source_recursive", StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr SceneFindSourceRecursive(IntPtr scene, string name);

    /// <summary>
    /// シーンアイテムの表示/非表示を設定
    /// </summary>
    [LibraryImport(ObsLib, EntryPoint = "obs_sceneitem_set_visible")]
    public static partial void SceneItemSetVisible(IntPtr sceneItem, [MarshalAs(UnmanagedType.I1)] bool visible);

    /// <summary>
    /// シーンアイテムの表示状態を取得
    /// </summary>
    [LibraryImport(ObsLib, EntryPoint = "obs_sceneitem_visible")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SceneItemVisible(IntPtr sceneItem);

    /// <summary>
    /// ソースの参照を解放
    /// </summary>
    [LibraryImport(ObsLib, EntryPoint = "obs_source_release")]
    public static partial void SourceRelease(ObsSourceContext source);

    #endregion

    #region Frontend API

    private const string FrontendLib = "obs-frontend-api";

    /// <summary>
    /// 現在のシーンを取得（obs_source_release()で解放が必要）
    /// </summary>
    [LibraryImport(FrontendLib, EntryPoint = "obs_frontend_get_current_scene")]
    public static partial ObsSourceContext FrontendGetCurrentScene();

    #endregion

    #region Output API

    [LibraryImport(ObsLib, EntryPoint = "obs_output_get_name")]
    public static partial IntPtr OutputGetName(ObsOutputContext output);

    [LibraryImport(ObsLib, EntryPoint = "obs_output_begin_data_capture")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool OutputBeginDataCapture(ObsOutputContext output, uint flags);

    [LibraryImport(ObsLib, EntryPoint = "obs_output_end_data_capture")]
    public static partial void OutputEndDataCapture(ObsOutputContext output);

    [LibraryImport(ObsLib, EntryPoint = "obs_output_signal_stop")]
    public static partial void OutputSignalStop(ObsOutputContext output, int code);

    #endregion

    #region Graphics API

    [LibraryImport(ObsLib, EntryPoint = "obs_enter_graphics")]
    public static partial void EnterGraphics();

    [LibraryImport(ObsLib, EntryPoint = "obs_leave_graphics")]
    public static partial void LeaveGraphics();

    [LibraryImport(ObsLib, EntryPoint = "gs_texture_create")]
    public static partial GsTexture GsTextureCreate(uint width, uint height, int format, uint levels, IntPtr data, uint flags);

    [LibraryImport(ObsLib, EntryPoint = "gs_texture_destroy")]
    public static partial void GsTextureDestroy(GsTexture texture);

    [LibraryImport(ObsLib, EntryPoint = "gs_texture_set_image")]
    public static partial void GsTextureSetImage(GsTexture texture, IntPtr data, uint linesize, [MarshalAs(UnmanagedType.I1)] bool flip);

    [LibraryImport(ObsLib, EntryPoint = "gs_effect_get_param_by_name", StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr GsEffectGetParamByName(GsEffect effect, string name);

    [LibraryImport(ObsLib, EntryPoint = "gs_effect_set_vec4")]
    public static partial void GsEffectSetVec4(IntPtr param, IntPtr val);

    [LibraryImport(ObsLib, EntryPoint = "gs_blend_state_push")]
    public static partial void GsBlendStatePush();

    [LibraryImport(ObsLib, EntryPoint = "gs_blend_state_pop")]
    public static partial void GsBlendStatePop();

    [LibraryImport(ObsLib, EntryPoint = "gs_blend_function_separate")]
    public static partial void GsBlendFunctionSeparate(int srcC, int destC, int srcA, int destA);

    [LibraryImport(ObsLib, EntryPoint = "obs_source_process_filter_tech_end", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void SourceProcessFilterTechEnd(ObsSourceContext filter, GsEffect effect, uint width, uint height, string techName);

    [LibraryImport(ObsLib, EntryPoint = "obs_get_base_effect")]
    public static partial GsEffect ObsGetBaseEffect(int effect);

    [LibraryImport(ObsLib, EntryPoint = "gs_effect_create_from_file", StringMarshalling = StringMarshalling.Utf8)]
    public static partial GsEffect GsEffectCreateFromFile(string file, IntPtr errorString);

    [LibraryImport(ObsLib, EntryPoint = "gs_effect_destroy")]
    public static partial void GsEffectDestroy(GsEffect effect);

    [LibraryImport(ObsLib, EntryPoint = "gs_effect_set_float", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GsEffectSetFloat(IntPtr param, float val);

    [LibraryImport(ObsLib, EntryPoint = "gs_effect_set_texture")]
    public static partial void GsEffectSetTexture(IntPtr param, GsTexture texture);

    [LibraryImport(ObsLib, EntryPoint = "gs_effect_get_technique", StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr GsEffectGetTechnique(GsEffect effect, string name);

    [LibraryImport(ObsLib, EntryPoint = "gs_technique_begin")]
    public static partial nuint GsTechniqueBegin(IntPtr technique);

    [LibraryImport(ObsLib, EntryPoint = "gs_technique_end")]
    public static partial void GsTechniqueEnd(IntPtr technique);

    [LibraryImport(ObsLib, EntryPoint = "gs_technique_begin_pass")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool GsTechniqueBeginPass(IntPtr technique, nuint pass);

    [LibraryImport(ObsLib, EntryPoint = "gs_technique_end_pass")]
    public static partial void GsTechniqueEndPass(IntPtr technique);

    [LibraryImport(ObsLib, EntryPoint = "gs_draw_sprite")]
    public static partial void GsDrawSprite(GsTexture texture, uint flip, uint width, uint height);

    [LibraryImport(ObsLib, EntryPoint = "obs_module_file", StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr ObsModuleFile(string file);

    #endregion

    #region Source Enable/Disable API

    /// <summary>
    /// ソースの有効/無効を設定
    /// </summary>
    [LibraryImport(ObsLib, EntryPoint = "obs_source_set_enabled")]
    public static partial void SourceSetEnabled(ObsSourceContext source, [MarshalAs(UnmanagedType.I1)] bool enabled);

    // SourceEnabled と FilterGetParent は既に Source API セクションに定義済み

    #endregion

    #region Audio API

    [LibraryImport(ObsLib, EntryPoint = "obs_get_audio_info")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool GetAudioInfo(ref ObsAudioInfo info);

    #endregion

    #region Utility API

    [LibraryImport(ObsLib, EntryPoint = "blog")]
    public static partial void Blog(int logLevel, IntPtr format);

    [LibraryImport(ObsLib, EntryPoint = "obs_get_version")]
    public static partial uint GetVersion();

    [LibraryImport(ObsLib, EntryPoint = "os_gettime_ns")]
    public static partial ulong OsGettimeNs();

    #endregion
}

/// <summary>
/// OBS log levels
/// </summary>
public static class LogLevel
{
    public const int Error = 100;
    public const int Warning = 200;
    public const int Info = 300;
    public const int Debug = 400;
}

/// <summary>
/// Graphics blend types (gs_blend_type)
/// </summary>
public enum GsBlendType
{
    Zero = 0,
    One = 1,
    SrcColor = 2,
    InvSrcColor = 3,
    SrcAlpha = 4,
    InvSrcAlpha = 5,
    DstAlpha = 6,
    InvDstAlpha = 7,
    DstColor = 8,
    InvDstColor = 9,
    SrcAlphaSat = 10
}

/// <summary>
/// OBS base effect types (obs_base_effect)
/// </summary>
public static class ObsBaseEffect
{
    public const int Default = 0;
    public const int DefaultRect = 1;
    public const int Opaque = 2;
    public const int SolidColor = 3;
    public const int Bicubic = 4;
    public const int Lanczos = 5;
    public const int Bilinear = 6;
    public const int Area = 7;
    public const int Premultiplied = 8;
}

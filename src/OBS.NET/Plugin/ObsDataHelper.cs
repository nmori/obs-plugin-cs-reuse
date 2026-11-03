using System.Runtime.InteropServices;
using OBS.NET.Native;

namespace OBS.NET.Plugin;

/// <summary>
/// Helper class for working with OBS data objects
/// </summary>
public static class ObsDataHelper
{
    /// <summary>
    /// Gets a string value from settings
    /// </summary>
    public static string? GetString(ObsData data, string name)
    {
        if (!data.IsValid) return null;
        var ptr = ObsApi.DataGetString(data, name);
        return ptr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(ptr);
    }

    /// <summary>
    /// Sets a string value in settings
    /// </summary>
    public static void SetString(ObsData data, string name, string value)
    {
        if (data.IsValid)
        {
            ObsApi.DataSetString(data, name, value);
        }
    }

    /// <summary>
    /// Gets an integer value from settings
    /// </summary>
    public static long GetInt(ObsData data, string name)
    {
        if (!data.IsValid) return 0;
        return ObsApi.DataGetInt(data, name);
    }

    /// <summary>
    /// Sets an integer value in settings
    /// </summary>
    public static void SetInt(ObsData data, string name, long value)
    {
        if (data.IsValid)
        {
            ObsApi.DataSetInt(data, name, value);
        }
    }

    /// <summary>
    /// Gets a double value from settings
    /// </summary>
    public static double GetDouble(ObsData data, string name)
    {
        if (!data.IsValid) return 0.0;
        return ObsApi.DataGetDouble(data, name);
    }

    /// <summary>
    /// Sets a double value in settings
    /// </summary>
    public static void SetDouble(ObsData data, string name, double value)
    {
        if (data.IsValid)
        {
            ObsApi.DataSetDouble(data, name, value);
        }
    }

    /// <summary>
    /// Gets a boolean value from settings
    /// </summary>
    public static bool GetBool(ObsData data, string name)
    {
        if (!data.IsValid) return false;
        return ObsApi.DataGetBool(data, name);
    }

    /// <summary>
    /// Sets a boolean value in settings
    /// </summary>
    public static void SetBool(ObsData data, string name, bool value)
    {
        if (data.IsValid)
        {
            ObsApi.DataSetBool(data, name, value);
        }
    }

    /// <summary>
    /// Sets a default string value
    /// </summary>
    public static void SetDefaultString(ObsData data, string name, string value)
    {
        if (data.IsValid)
        {
            ObsApi.DataSetDefaultString(data, name, value);
        }
    }

    /// <summary>
    /// Sets a default integer value
    /// </summary>
    public static void SetDefaultInt(ObsData data, string name, long value)
    {
        if (data.IsValid)
        {
            ObsApi.DataSetDefaultInt(data, name, value);
        }
    }

    /// <summary>
    /// Sets a default double value
    /// </summary>
    public static void SetDefaultDouble(ObsData data, string name, double value)
    {
        if (data.IsValid)
        {
            ObsApi.DataSetDefaultDouble(data, name, value);
        }
    }

    /// <summary>
    /// Sets a default boolean value
    /// </summary>
    public static void SetDefaultBool(ObsData data, string name, bool value)
    {
        if (data.IsValid)
        {
            ObsApi.DataSetDefaultBool(data, name, value);
        }
    }
}

/// <summary>
/// Helper class for building OBS properties
/// </summary>
public class ObsPropertiesBuilder
{
    private readonly ObsProperties _props;

    public ObsPropertiesBuilder()
    {
        _props = ObsApi.PropertiesCreate();
    }

    /// <summary>
    /// Gets the built properties object
    /// </summary>
    public ObsProperties Build() => _props;

    /// <summary>
    /// Adds a boolean property
    /// </summary>
    public ObsPropertiesBuilder AddBool(string name, string description)
    {
        ObsApi.PropertiesAddBool(_props, name, description);
        return this;
    }

    /// <summary>
    /// Adds an integer property with range
    /// </summary>
    public ObsPropertiesBuilder AddInt(string name, string description, int min, int max, int step = 1)
    {
        ObsApi.PropertiesAddInt(_props, name, description, min, max, step);
        return this;
    }

    /// <summary>
    /// Adds a float/double property with range
    /// </summary>
    public ObsPropertiesBuilder AddFloat(string name, string description, double min, double max, double step = 0.01)
    {
        ObsApi.PropertiesAddFloat(_props, name, description, min, max, step);
        return this;
    }

    /// <summary>
    /// Adds a text property
    /// </summary>
    public ObsPropertiesBuilder AddText(string name, string description, TextType type = TextType.Default)
    {
        ObsApi.PropertiesAddText(_props, name, description, (int)type);
        return this;
    }

    /// <summary>
    /// Adds a path property
    /// </summary>
    public ObsPropertiesBuilder AddPath(string name, string description, PathType type, string filter = "", string defaultPath = "")
    {
        ObsApi.PropertiesAddPath(_props, name, description, (int)type, filter, defaultPath);
        return this;
    }

    /// <summary>
    /// Adds a color property
    /// </summary>
    public ObsPropertiesBuilder AddColor(string name, string description)
    {
        ObsApi.PropertiesAddColor(_props, name, description);
        return this;
    }

    /// <summary>
    /// Adds a font property
    /// </summary>
    public ObsPropertiesBuilder AddFont(string name, string description)
    {
        ObsApi.PropertiesAddFont(_props, name, description);
        return this;
    }

    /// <summary>
    /// Adds a list property with string values
    /// </summary>
    public ObsPropertiesBuilder AddList(string name, string description, ListType type, Action<ObsProperty> configure)
    {
        var prop = ObsApi.PropertiesAddList(_props, name, description, (int)type, (int)ListFormat.String);
        configure(prop);
        return this;
    }

    /// <summary>
    /// Adds a list property with integer values
    /// </summary>
    public ObsPropertiesBuilder AddListInt(string name, string description, ListType type, Action<ObsProperty> configure)
    {
        var prop = ObsApi.PropertiesAddList(_props, name, description, (int)type, (int)ListFormat.Int);
        configure(prop);
        return this;
    }

    /// <summary>
    /// Adds a button property with callback
    /// </summary>
    public ObsPropertiesBuilder AddButton(string name, string description, ButtonCallback callback)
    {
        var callbackPtr = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(callback);
        ObsApi.PropertiesAddButton(_props, name, description, callbackPtr);
        // Store delegate to prevent garbage collection
        _buttonCallbacks.Add(callback);
        return this;
    }

    /// <summary>
    /// Adds a boolean property with modified callback
    /// </summary>
    public ObsPropertiesBuilder AddBoolWithCallback(string name, string description, ObsPropertyModifiedCallback callback)
    {
        var prop = ObsApi.PropertiesAddBool(_props, name, description);
        var callbackPtr = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(callback);
        ObsApi.PropertySetModifiedCallback(prop, callbackPtr);
        // Store delegate to prevent garbage collection
        _modifiedCallbacks.Add(callback);
        return this;
    }

    /// <summary>
    /// Adds a list property with string values and modified callback
    /// </summary>
    public ObsPropertiesBuilder AddListWithCallback(string name, string description, ListType type, ObsPropertyModifiedCallback callback, Action<ObsProperty> configure)
    {
        var prop = ObsApi.PropertiesAddList(_props, name, description, (int)type, (int)ListFormat.String);
        configure(prop);
        var callbackPtr = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(callback);
        ObsApi.PropertySetModifiedCallback(prop, callbackPtr);
        // Store delegate to prevent garbage collection
        _modifiedCallbacks.Add(callback);
        return this;
    }

    /// <summary>
    /// Adds a list property with integer values and modified callback
    /// </summary>
    public ObsPropertiesBuilder AddListIntWithCallback(string name, string description, ListType type, ObsPropertyModifiedCallback callback, Action<ObsProperty> configure)
    {
        var prop = ObsApi.PropertiesAddList(_props, name, description, (int)type, (int)ListFormat.Int);
        configure(prop);
        var callbackPtr = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(callback);
        ObsApi.PropertySetModifiedCallback(prop, callbackPtr);
        // Store delegate to prevent garbage collection
        _modifiedCallbacks.Add(callback);
        return this;
    }

    /// <summary>
    /// Gets the properties handle (for use in callbacks)
    /// </summary>
    public ObsProperties Properties => _props;

    /// <summary>
    /// Sets visibility of a property by name
    /// </summary>
    public static void SetPropertyVisible(ObsProperties props, string name, bool visible)
    {
        var prop = ObsApi.PropertiesGet(props, name);
        if (prop.IsValid)
        {
            ObsApi.PropertySetVisible(prop, visible);
        }
    }

    /// <summary>
    /// Adds a group property (collapsible section)
    /// </summary>
    public ObsPropertiesBuilder AddGroup(string name, string description, GroupType type, Action<ObsPropertiesBuilder> configure)
    {
        var groupBuilder = new ObsPropertiesBuilder();
        configure(groupBuilder);
        ObsApi.PropertiesAddGroup(_props, name, description, (int)type, groupBuilder.Properties);
        return this;
    }

    private readonly List<ButtonCallback> _buttonCallbacks = new();
    private readonly List<ObsPropertyModifiedCallback> _modifiedCallbacks = new();
}

/// <summary>
/// Button callback delegate for OBS properties
/// </summary>
[System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl)]
public delegate bool ButtonCallback(IntPtr props, IntPtr property, IntPtr data);

/// <summary>
/// Property modified callback delegate for OBS properties
/// Return true to request a UI refresh (e.g., when visibility changed)
/// </summary>
[System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl)]
public delegate bool ObsPropertyModifiedCallback(ObsProperties props, ObsProperty property, ObsData settings);

/// <summary>
/// Text input types
/// </summary>
public enum TextType
{
    Default = 0,
    Password = 1,
    Multiline = 2,
    Info = 3
}

/// <summary>
/// Path selection types
/// </summary>
public enum PathType
{
    File = 0,
    FileSave = 1,
    Directory = 2
}

/// <summary>
/// List display types (obs_combo_type)
/// </summary>
public enum ListType
{
    Invalid = 0,
    Editable = 1,  // User can type (must use ListFormat.String)
    List = 2       // Non-editable dropdown
}

/// <summary>
/// List value format (obs_combo_format)
/// </summary>
public enum ListFormat
{
    Invalid = 0,
    Int = 1,
    Float = 2,
    String = 3
}

/// <summary>
/// Group types for property grouping (obs_group_type)
/// </summary>
public enum GroupType
{
    /// <summary>
    /// Invalid - do not use
    /// </summary>
    Invalid = 0,
    /// <summary>
    /// Normal group - always visible, can be collapsed
    /// </summary>
    Normal = 1,
    /// <summary>
    /// Checkable group - has a checkbox, content shown only when checked
    /// </summary>
    Checkable = 2
}

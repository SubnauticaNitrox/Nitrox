using System;
using System.Globalization;
using UnityEngine;

namespace NitroxClient.Debuggers.Drawer;

// Reference https://gist.github.com/Seneral/31161381f993a4a06c59bf12576cabd8#file-rteditorgui-cs
public static class NitroxGUILayout
{
    private static int activeConvertibleField = -1;
    private static IConvertible activeConvertibleFieldLastValue = 0;
    private static string activeConvertibleFieldString = "";

    private static readonly GUIStyle separatorStyle = new() { stretchWidth = true };
    public static readonly GUIStyle DrawerLabel = new("options_label") { fixedHeight = 22, alignment = TextAnchor.LowerLeft };

    public const float VALUE_WIDTH = 175;
    public const float DEFAULT_LABEL_WIDTH = 200;
    public const float DEFAULT_SPACE = 10;

    public static void Separator()
    {
        GUILayout.Box(GUIContent.none, separatorStyle, GUILayout.Height(1));
    }

    public static int IntField(int value, float valueWidth = VALUE_WIDTH) => ConvertibleField(value, valueWidth).ToInt32(CultureInfo.CurrentCulture);
    public static float FloatField(float value, float valueWidth = VALUE_WIDTH) => ConvertibleField(value, valueWidth).ToSingle(CultureInfo.CurrentCulture);

    public static IConvertible ConvertibleField(IConvertible value, float valueWidth = VALUE_WIDTH)
    {
        int floatFieldID = GUIUtility.GetControlID("ConvertibleField".GetHashCode(), FocusType.Keyboard) + 1;
        if (floatFieldID == 0)
        {
            return value;
        }

        bool recorded = activeConvertibleField == floatFieldID;
        bool active = floatFieldID == GUIUtility.keyboardControl;

        if (active && recorded && !Equals(activeConvertibleFieldLastValue, value))
        {
            // Value has been modified externally
            activeConvertibleFieldLastValue = value;
            activeConvertibleFieldString = value.ToString(CultureInfo.CurrentCulture);
        }

        // Get stored string for the text field if this one is recorded
        string str = recorded ? activeConvertibleFieldString : value.ToString(CultureInfo.CurrentCulture);

        string strValue = GUILayout.TextField(str, GUILayout.Width(valueWidth));
        if (recorded)
        {
            activeConvertibleFieldString = strValue;
        }

        // Try Parse if value got changed. If the string could not be parsed, ignore it and keep last value
        bool parsed = true;
        if (string.IsNullOrEmpty(strValue))
        {
            value = activeConvertibleFieldLastValue = 0;
        }
        else if (strValue != value.ToString(CultureInfo.CurrentCulture))
        {
            parsed = TryParseIConvertible(value, strValue, out IConvertible newValue);
            if (parsed)
            {
                value = activeConvertibleFieldLastValue = newValue;
            }
        }

        switch (active)
        {
            case true when !recorded: // Gained focus this frame
                activeConvertibleField = floatFieldID;
                activeConvertibleFieldString = strValue;
                activeConvertibleFieldLastValue = value;
                break;
            case false when recorded: // Lost focus this frame
            {
                activeConvertibleField = -1;
                if (parsed)
                {
                    break;
                }

                value = TryParseIConvertible(value, strValue, out IConvertible newValue) ? newValue : activeConvertibleFieldLastValue;
                break;
            }
        }

        return value;
    }

    private static bool TryParseIConvertible(IConvertible type, string inputString, out IConvertible newValue)
    {
        bool parsed;
        switch (type)
        {
            case short:
                parsed = short.TryParse(inputString, NumberStyles.Integer, CultureInfo.CurrentCulture, out short newShort);
                newValue = newShort;
                break;
            case ushort:
                parsed = ushort.TryParse(inputString, NumberStyles.Integer, CultureInfo.CurrentCulture, out ushort newUShort);
                newValue = newUShort;
                break;
            case int:
                parsed = int.TryParse(inputString, NumberStyles.Integer, CultureInfo.CurrentCulture, out int newInt);
                newValue = newInt;
                break;
            case uint _:
                parsed = uint.TryParse(inputString, NumberStyles.Integer, CultureInfo.CurrentCulture, out uint newUInt);
                newValue = newUInt;
                break;
            case long:
                parsed = long.TryParse(inputString, NumberStyles.Integer, CultureInfo.CurrentCulture, out long newLong);
                newValue = newLong;
                break;
            case ulong:
                parsed = ulong.TryParse(inputString, NumberStyles.Integer, CultureInfo.CurrentCulture, out ulong newULong);
                newValue = newULong;
                break;
            case float:
                parsed = float.TryParse(inputString, NumberStyles.Float, CultureInfo.CurrentCulture, out float newFloat);
                newValue = newFloat;
                break;
            case double:
                parsed = double.TryParse(inputString, NumberStyles.Float, CultureInfo.CurrentCulture, out double newDouble);
                newValue = newDouble;
                break;
            default:
                parsed = false;
                newValue = null;
                break;
        }

        return parsed;
    }

    public static int SliderField(int value, int minValue, int maxValue, float valueWidth = VALUE_WIDTH) => (int)SliderField((float)value, minValue, maxValue, valueWidth);

    public static float SliderField(float value, float minValue, float maxValue, float valueWidth = VALUE_WIDTH)
    {
        //TODO: Implement slider (if possible at all)
        return Math.Max(minValue, Math.Min(maxValue, FloatField(value, valueWidth)));
    }

    public static T EnumPopup<T>(T selected, float buttonWidth = VALUE_WIDTH) where T : Enum
    {
        //TODO: Implement selecting of Enum in new window
        Color oldColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.cyan;
        GUILayout.Button(selected.ToString(), GUILayout.MaxWidth(buttonWidth));
        GUI.backgroundColor = oldColor;
        return selected;
    }

    public static bool BoolField(bool value, float valueWidth = VALUE_WIDTH) => BoolFieldInternal(value, value.ToString(), valueWidth);
    public static bool BoolField(bool value, string name, float valueWidth = VALUE_WIDTH) => BoolFieldInternal(value, $"{name}: {value}", valueWidth);

    private static bool BoolFieldInternal(bool value, string buttonLabel, float valueWidth = VALUE_WIDTH)
    {
        if (GUILayout.Button(buttonLabel, GUILayout.Width(valueWidth)))
        {
            return !value;
        }

        return value;
    }

    public struct BackgroundColorScope : IDisposable
    {
        private bool disposed;
        private readonly Color previousColor;

        public BackgroundColorScope(Color newColor)
        {
            disposed = false;
            previousColor = GUI.color;
            GUI.backgroundColor = newColor;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            GUI.backgroundColor = previousColor;
        }
    }
}

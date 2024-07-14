using System.Reflection;
using Avalonia.Collections;
using NitroxModel.Serialization;

namespace Nitrox.Launcher.Models.Design;

public record EditorField
{
    public object Value { get; set; }

    public PropertyInfo PropertyInfo { get; init; }

    public AvaloniaList<object> PossibleValues { get; set; }

    public string Description
    {
        get
        {
            string description = PropertyInfo.GetCustomAttribute<PropertyDescriptionAttribute>()?.Description;
            if (string.IsNullOrWhiteSpace(description))
            {
                description = null;
            }
            return description;
        }
    }

    public EditorField(PropertyInfo propertyInfo, object value, AvaloniaList<object> possibleValues)
    {
        PropertyInfo = propertyInfo;
        Value = value;
        PossibleValues = possibleValues;
    }
}

using System.Reflection;
using NitroxModel.Serialization;

namespace Nitrox.Launcher.Models.Design;

public class EditorField
{
    public object Value { get; set; }

    public PropertyInfo PropertyInfo { get; init; }

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

    public EditorField(PropertyInfo propertyInfo, object value)
    {
        PropertyInfo = propertyInfo;
        Value = value;
    }
}

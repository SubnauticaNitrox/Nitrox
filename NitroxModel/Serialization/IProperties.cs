using System;
using System.ComponentModel;

namespace NitroxModel.Serialization
{
    public interface IProperties
    {
        public string FileName { get; }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, Inherited = false)]
    public sealed class PropertyDescriptionAttribute : DescriptionAttribute
    {
        public PropertyDescriptionAttribute(string desc) : base(desc)
        {
        }

        public PropertyDescriptionAttribute(string desc, Type type)
        {
            if (type.IsEnum)
            {
                desc += $" {string.Join(", ", type.GetEnumNames())}";
                DescriptionValue = desc;
            }
        }
    }
}

using System;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace NitroxModel.Serialization
{
    public interface IProperties
    {
        public string FileName { get; }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PropertyDescriptionAttribute : DescriptionAttribute
    {
        public PropertyDescriptionAttribute(string desc) : base(desc)
        {}

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

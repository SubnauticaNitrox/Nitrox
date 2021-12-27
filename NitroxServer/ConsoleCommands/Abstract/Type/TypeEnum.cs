using System;
using NitroxModel.Helper;

namespace NitroxServer.ConsoleCommands.Abstract.Type
{
    public class TypeEnum<T> : Parameter<object> where T : struct, Enum
    {
        public TypeEnum(string name, bool required, string description) : base(name, required, description)
        {
            Validate.IsTrue(typeof(T).IsEnum, $"Type {typeof(T).FullName} isn't an enum");
        }

        public override bool IsValid(string arg)
        {
            return Enum.TryParse<T>(arg, true, out _);
        }

        public override object Read(string arg)
        {
            Validate.IsTrue(Enum.TryParse(arg, true, out T value), "Unknown value received");
            return value;
        }

        public override string GetDescription()
        {
            return $"{base.GetDescription()} (values: {string.Join(", ", Enum.GetNames(typeof(T)))})";
        }
    }
}

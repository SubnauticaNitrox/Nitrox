using System;
using NitroxModel.Helper;

namespace NitroxServer.ConsoleCommands.Abstract.Type
{
    public class TypeEnum<T> : Parameter<object> where T : struct
    {
        public TypeEnum(string name, bool required) : base(name, required)
        {
            Validate.IsTrue(typeof(T).IsEnum, $"Type {typeof(T).FullName} isn't an enum");
        }

        public override bool IsValid(string arg)
        {
            T result;
            return Enum.TryParse(arg, true, out result);
        }

        public override object Read(string arg)
        {
            T value;

            Validate.IsTrue(Enum.TryParse(arg, true, out value), "Unknown value received");

            return value;
        }
    }
}

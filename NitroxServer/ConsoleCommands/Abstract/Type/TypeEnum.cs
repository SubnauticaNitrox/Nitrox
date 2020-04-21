using System;
using NitroxServer.Exceptions;

namespace NitroxServer.ConsoleCommands.Abstract.Type
{
    public class TypeEnum<T> : Parameter<object> where T : struct
    {

        public TypeEnum(string name, bool required) : base(name, required)
        {
            if (!typeof(T).IsEnum)
            {
                throw new IllegalArgumentException($"Type {typeof(T).FullName} isn't an enum");
            }
        }

        public override bool IsValid(string arg)
        {
            T result;
            return Enum.TryParse(arg, true, out result);
        }

        public override object Read(string arg)
        {
            T value;
            if (!Enum.TryParse(arg, true, out value))
            {
                throw new IllegalArgumentException("Unknown value received");
            }

            return value;
        }
    }
}

using System;
using NitroxServer.Exceptions;

namespace NitroxServer.ConsoleCommands.Abstract
{
    public class TypeEnum<T> : TypeAbstract<T> where T : struct
    {

        public TypeEnum()
        {
            if (!typeof(T).IsEnum)
            {
                throw new IllegalArgumentException($"Type {typeof(T).FullName} isn't an enum");
            }
        }

        public override bool isValid(string arg)
        {
            //Let's suppose enum are in UPPERCASE
            return Enum.IsDefined(typeof(T), arg.ToUpper());
        }

        public override T read(string arg)
        {
            T _;

            if (!Enum.TryParse(arg.ToUpper(), out _))
            {
                throw new IllegalArgumentException("Invalid value received");
            }

            return _;
        }
    }
}

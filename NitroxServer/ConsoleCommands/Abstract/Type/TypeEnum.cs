using System;
using NitroxServer.Exceptions;

namespace NitroxServer.ConsoleCommands.Abstract
{
    public class TypeEnum<T> : TypeAbstract<T> where T : struct
    {

        public TypeEnum()
        {
            if (!GetType().IsEnum)
            {
                throw new IllegalArgumentException($"Type {GetType().Name} isn't an enum");
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

            if (!Enum.TryParse(arg, out _))
            {
                throw new IllegalArgumentException("Invalid Enum string received");
            }

            return _;
        }
    }
}

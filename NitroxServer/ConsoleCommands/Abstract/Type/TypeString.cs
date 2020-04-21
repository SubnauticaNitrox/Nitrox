using NitroxServer.Exceptions;

namespace NitroxServer.ConsoleCommands.Abstract.Type
{
    public class TypeString : Parameter<string>
    {
        public TypeString(string name, bool isRequired) : base(name, isRequired) { }

        public override bool IsValid(string arg)
        {
            return !string.IsNullOrEmpty(arg);
        }

        public override string Read(string arg)
        {
            if (!IsValid(arg))
            {
                throw new IllegalArgumentException("Received null/empty instead of a valid string");
            }

            return arg;
        }
    }
}

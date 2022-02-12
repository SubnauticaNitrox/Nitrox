using NitroxModel.Helper;

namespace NitroxServer.ConsoleCommands.Abstract.Type
{
    public class TypeString : Parameter<string>
    {
        public TypeString(string name, bool isRequired, string description) : base(name, isRequired, description) { }

        public override bool IsValid(string arg)
        {
            return !string.IsNullOrEmpty(arg);
        }

        public override string Read(string arg)
        {
            Validate.IsTrue(IsValid(arg), "Received null/empty instead of a valid string");
            return arg;
        }
    }
}

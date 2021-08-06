using NitroxModel.Helper;

namespace NitroxServer.ConsoleCommands.Abstract.Type
{
    public class TypeInt : Parameter<int>, IParameter<object>
    {
        public TypeInt(string name, bool isRequired, string description) : base(name, isRequired, description) { }

        public override bool IsValid(string arg)
        {
            return int.TryParse(arg, out _);
        }

        public override int Read(string arg)
        {
            Validate.IsTrue(int.TryParse(arg, out int value), "Invalid integer received");
            return value;
        }

        object IParameter<object>.Read(string arg)
        {
            return Read(arg);
        }
    }
}

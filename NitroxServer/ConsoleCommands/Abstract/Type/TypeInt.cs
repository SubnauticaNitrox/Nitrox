using NitroxModel.Helper;

namespace NitroxServer.ConsoleCommands.Abstract.Type
{
    public class TypeInt : Parameter<int?>, IParameter<object>
    {
        public TypeInt(string name, bool isRequired) : base(name, isRequired) { }

        public override bool IsValid(string arg)
        {
            int value;
            return int.TryParse(arg, out value);
        }

        public override int? Read(string arg)
        {
            int value;

            Validate.IsTrue(int.TryParse(arg, out value), "Invalid integer received");

            return value;
        }

        object IParameter<object>.Read(string arg)
        {
            return Read(arg);
        }
    }
}

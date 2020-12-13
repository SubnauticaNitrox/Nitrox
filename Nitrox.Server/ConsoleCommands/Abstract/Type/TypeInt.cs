using Nitrox.Model.Helper;

namespace Nitrox.Server.ConsoleCommands.Abstract.Type
{
    public class TypeInt : Parameter<int?>, IParameter<object>
    {
        public TypeInt(string name, bool isRequired) : base(name, isRequired) { }

        public override bool IsValid(string arg)
        {
            return int.TryParse(arg, out int value);
        }

        public override int? Read(string arg)
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

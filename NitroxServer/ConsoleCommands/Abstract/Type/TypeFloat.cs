using NitroxModel.Helper;

namespace NitroxServer.ConsoleCommands.Abstract.Type
{
    public class TypeFloat : Parameter<float>, IParameter<object>
    {
        public TypeFloat(string name, bool isRequired, string description) : base(name, isRequired, description) { }

        public override bool IsValid(string arg)
        {
            return float.TryParse(arg, out _);
        }

        public override float Read(string arg)
        {
            Validate.IsTrue(float.TryParse(arg, out float value), "Invalid decimal number received");
            return value;
        }

        object IParameter<object>.Read(string arg)
        {
            return Read(arg);
        }
    }
}

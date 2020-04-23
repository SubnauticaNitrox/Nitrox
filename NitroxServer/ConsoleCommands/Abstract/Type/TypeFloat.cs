using NitroxModel.Helper;

namespace NitroxServer.ConsoleCommands.Abstract.Type
{
    public class TypeFloat : Parameter<float?>, IParameter<object>
    {
        object IParameter<object>.DefaultValue => 0f;
        object IParameter<object>.Read(string arg) => Read(arg);

        public TypeFloat(string name, bool isRequired) : base(name, isRequired) { }

        public override bool IsValid(string arg)
        {
            float value;
            return float.TryParse(arg, out value);
        }

        public override float? Read(string arg)
        {
            float value;

            Validate.IsTrue(float.TryParse(arg, out value), "Invalid decimal number received");

            return value;
        }
    }
}

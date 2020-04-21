using NitroxServer.Exceptions;

namespace NitroxServer.ConsoleCommands.Abstract.Type
{
    public class TypeFloat : Parameter<float>, IParameter<object>
    {
        object IParameter<object>.DefaultValue => 0f;
        object IParameter<object>.Read(string arg) => Read(arg);

        public TypeFloat(string name, bool isRequired) : base(name, isRequired) { }

        public override bool IsValid(string arg)
        {
            float value;
            return float.TryParse(arg, out value);
        }

        public override float Read(string arg)
        {
            float value;
            if (!float.TryParse(arg, out value))
            {
                throw new IllegalArgumentException("Invalid decimal number received");
            }

            return value;
        }
    }
}

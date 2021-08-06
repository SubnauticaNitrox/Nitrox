using System;
using System.Linq;
using NitroxModel.Helper;

namespace NitroxServer.ConsoleCommands.Abstract.Type
{
    public class TypeBoolean : Parameter<bool>, IParameter<object>
    {
        private static readonly string[] noValues = new string[]
        {
            bool.FalseString,
            "no",
            "off"
        };

        private static readonly string[] yesValues = new string[]
        {
            bool.TrueString,
            "yes",
            "on"
        };

        public TypeBoolean(string name, bool isRequired, string description) : base(name, isRequired, description) { }

        public override bool IsValid(string arg)
        {
            return yesValues.Contains(arg, StringComparer.OrdinalIgnoreCase) || noValues.Contains(arg, StringComparer.OrdinalIgnoreCase);
        }

        public override bool Read(string arg)
        {
            Validate.IsTrue(IsValid(arg), "Invalid boolean value received");
            return yesValues.Contains(arg, StringComparer.OrdinalIgnoreCase);
        }

        object IParameter<object>.Read(string arg)
        {
            return Read(arg);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.Helper;

namespace NitroxServer.ConsoleCommands.Abstract.Type
{
    public class TypeBoolean : Parameter<bool?>, IParameter<object>
    {
        private static readonly IList<string> noValues = new List<string>
        {
            bool.FalseString,
            "no",
            "off"
        };

        private static readonly IList<string> yesValues = new List<string>
        {
            bool.TrueString,
            "yes",
            "on"
        };

        public TypeBoolean(string name, bool isRequired) : base(name, isRequired) { }

        public override bool IsValid(string arg)
        {
            return yesValues.Contains(arg, StringComparer.OrdinalIgnoreCase) || noValues.Contains(arg, StringComparer.OrdinalIgnoreCase);
        }

        public override bool? Read(string arg)
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

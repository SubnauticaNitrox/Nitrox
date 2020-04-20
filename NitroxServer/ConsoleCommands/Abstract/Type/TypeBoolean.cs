using System;
using System.Collections.Generic;
using System.Linq;
using NitroxServer.Exceptions;

namespace NitroxServer.ConsoleCommands.Abstract.Type
{
    public class TypeBoolean : Parameter<bool?>, IParameter<object>
    {
        private readonly IList<string> noValues = new List<string>
        {
            bool.FalseString,
            "no",
            "off"
        };

        private readonly IList<string> yesValues = new List<string>
        {
            bool.TrueString,
            "yes",
            "on"
        };

        public TypeBoolean(string name, bool isRequired) : base(name, isRequired)
        {
        }

        object IParameter<object>.DefaultValue => null;

        object IParameter<object>.Read(string arg)
        {
            return Read(arg);
        }

        public override bool? Read(string arg)
        {
            if (!IsValid(arg))
            {
                throw new IllegalArgumentException("Invalid boolean value received");
            }

            return yesValues.Contains(arg, StringComparer.OrdinalIgnoreCase);
        }

        public override bool IsValid(string arg)
        {
            return yesValues.Contains(arg, StringComparer.OrdinalIgnoreCase) || noValues.Contains(arg, StringComparer.OrdinalIgnoreCase);
        }
    }
}

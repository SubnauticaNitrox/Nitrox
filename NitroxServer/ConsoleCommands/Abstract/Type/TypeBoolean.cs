using System;
using System.Collections.Generic;
using System.Linq;
using NitroxServer.Exceptions;

namespace NitroxServer.ConsoleCommands.Abstract
{
    public class TypeBoolean : TypeAbstract<bool?>
    {
        #region Singleton
        private static TypeBoolean get;

        public static TypeBoolean Get
        {
            get
            {
                return get ?? (get = new TypeBoolean());
            }
        }
        #endregion

        private readonly IList<string> yesValues = new List<string>()
        {
            bool.TrueString, "yes", "on"
        };

        private readonly IList<string> noValues = new List<string>()
        {
            bool.FalseString, "no", "off",
        };

        public override bool isValid(string arg)
        {
            return yesValues.Contains(arg, StringComparer.OrdinalIgnoreCase) || noValues.Contains(arg, StringComparer.OrdinalIgnoreCase);
        }

        public override bool? read(string arg)
        {
            if (!isValid(arg))
            {
                throw new IllegalArgumentException("Invalid boolean value received");
            }

            return yesValues.Contains(arg, StringComparer.OrdinalIgnoreCase);
        }
    }
}

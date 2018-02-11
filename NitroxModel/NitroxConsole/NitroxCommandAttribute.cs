using System;
using NitroxModel.Helper;

namespace NitroxModel.NitroxConsole
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class NitroxCommandAttribute : Attribute
    {
        public CommandIdentity Identity { get; }

        public NitroxCommandAttribute(string command) : this(NitroxConsole.DEFAULT_CATEGORY, command)
        {
        }

        public NitroxCommandAttribute(string category, string command) : this(new CommandIdentity(category, command))
        {
        }

        public NitroxCommandAttribute(CommandIdentity identity)
        {
            Validate.NotNull(identity);

            Identity = identity;
        }

        public override string ToString()
        {
            return $"{nameof(Identity)}: {Identity}";
        }
    }
}

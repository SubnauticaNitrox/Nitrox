using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.Helper;

namespace NitroxModel.NitroxConsole
{
    public class CommandIdentity : IEquatable<CommandIdentity>
    {
        /// <summary>
        /// Category that the command belongs to.
        /// </summary>
        public string Category { get; }
        /// <summary>
        /// Name of the command.
        /// </summary>
        public string Command { get; }

        public CommandIdentity(string category, string command)
        {
            Validate.NotNullOrWhitespace(category);
            Validate.NotNullOrWhitespace(command);

            if (category.Any(char.IsUpper))
            {
                throw new NotSupportedException("The category of a command must be all lower-case.");
            }

            if (command.Any(char.IsUpper))
            {
                throw new NotSupportedException("Command name must be all lower-case.");
            }

            Category = category;
            Command = command;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CommandIdentity);
        }

        public bool Equals(CommandIdentity other)
        {
            return other != null &&
                   Category == other.Category &&
                   Command == other.Command;
        }

        public override int GetHashCode()
        {
            int hashCode = 1403680907;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Category);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Command);
            return hashCode;
        }

        public static bool operator ==(CommandIdentity identity1, CommandIdentity identity2)
        {
            return EqualityComparer<CommandIdentity>.Default.Equals(identity1, identity2);
        }

        public static bool operator !=(CommandIdentity identity1, CommandIdentity identity2)
        {
            return !(identity1 == identity2);
        }

        public override string ToString()
        {
            return $"{nameof(Category)}: {Category}, {nameof(Command)}: {Command}";
        }
    }
}

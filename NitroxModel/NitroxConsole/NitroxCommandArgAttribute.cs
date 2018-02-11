using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.Helper;

namespace NitroxModel.NitroxConsole
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class NitroxCommandArgAttribute : Attribute, IEquatable<NitroxCommandArgAttribute>
    {
        private string[] aliases;
        private string description;

        public string Name { get; private set; }
        public CommandArgInput.Type Type { get; }
        public bool Required { get; private set; }

        /// <summary>
        ///     Aliases of the argument. Usage: [CommandCategory] [CommandName] -[alias1|alias2|alias3..]
        /// </summary>
        public string[] Aliases
        {
            get { return aliases; }
            protected set { aliases = value.Where(v => v != null).Select(v => v.Trim().ToLowerInvariant()).ToArray(); }
        }

        /// <summary>
        ///     Description of what the argument means for its command.
        /// </summary>
        public string Description
        {
            get { return description ?? ""; }
            protected set { description = value; }
        }

        public NitroxCommandArgAttribute(string name, CommandArgInput.Type type, bool required, params string[] aliases) : this(name, type, required, aliases, null)
        {

        }

        public NitroxCommandArgAttribute(string name, CommandArgInput.Type type, params string[] aliases) : this(name, type, false, aliases, null)
        {

        }

        public NitroxCommandArgAttribute(string name, CommandArgInput.Type type, bool required, string[] aliases, string description)
        {
            Validate.NotNullOrEmpty(name);
            aliases = aliases ?? new string[0];

            if (aliases.Select(a => a.ToLowerInvariant()).Distinct().Count() != aliases.Length)
            {
                throw new ArgumentException("Aliases must be unique among each other.", nameof(aliases));
            }

            if (aliases.Any(v => string.IsNullOrEmpty(v) || v.All(c => c == ' ')))
            {
                throw new ArgumentException("Item in aliases must not be null or empty.", nameof(aliases));
            }

            if (aliases.Any(v => v.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Aliases must not contain a value that equals the name.", nameof(aliases));
            }

            Name = name.ToLowerInvariant();
            Type = type;
            Aliases = aliases;
            Description = description;
            Required = required;
        }

        public bool HasName(string argName)
        {
            if (argName == Name)
            {
                return true;
            }

            if (Array.IndexOf(Aliases, argName) >= 0)
            {
                return true;
            }

            return false;
        }

        public bool IsSameCommandArg(NitroxCommandArgAttribute other)
        {
            return Name == other.Name && Type == other.Type && Required == other.Required;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as NitroxCommandArgAttribute);
        }

        public bool Equals(NitroxCommandArgAttribute other)
        {
            return other != null &&
                   base.Equals(other) &&
                   EqualityComparer<string[]>.Default.Equals(aliases, other.aliases) &&
                   description == other.description &&
                   Name == other.Name &&
                   Type == other.Type &&
                   Required == other.Required &&
                   EqualityComparer<string[]>.Default.Equals(Aliases, other.Aliases) &&
                   Description == other.Description;
        }

        public override int GetHashCode()
        {
            int hashCode = 1691396427;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(aliases);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(description);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + Required.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(Aliases);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
            return hashCode;
        }

        public static bool operator ==(NitroxCommandArgAttribute attribute1, NitroxCommandArgAttribute attribute2)
        {
            return EqualityComparer<NitroxCommandArgAttribute>.Default.Equals(attribute1, attribute2);
        }

        public static bool operator !=(NitroxCommandArgAttribute attribute1, NitroxCommandArgAttribute attribute2)
        {
            return !(attribute1 == attribute2);
        }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Type)}: {Type}, {nameof(Required)}: {Required}, {nameof(Aliases)}: {Aliases}";
        }
    }
}

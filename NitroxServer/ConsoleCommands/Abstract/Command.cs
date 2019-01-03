using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;

namespace NitroxServer.ConsoleCommands.Abstract
{
    public abstract class Command
    {
        public Optional<string> Args { get; protected set; }
        public string Name { get; protected set; }
        public string[] Alias { get; protected set; }
        public string Description { get; protected set; }

        protected Command(string name) : this(name, Optional<string>.Empty(), "", null)
        {
            Name = name;
        }

        protected Command(string name, Optional<string> args) : this(name, args, "", null)
        {
            Args = args;
            Name = name;
        }

        protected Command(string name, Optional<string> args, string description) : this(name, args, "", null)
        {
            Args = args;
            Name = name;
            Description = description;
        }

        protected Command(string name, Optional<string> args, string description, string[] alias)
        {
            Validate.NotNull(name);
            Validate.NotNull(args);

            Name = name;
            if (args.IsEmpty())
            {
                args = Optional<string>.Of(name);
            }

            Description = description ?? "";
            Args = args;
            Alias = alias ?? new string[0];
        }

        /// <summary>
        ///     Runs your command
        /// </summary>
        /// <param name="args">
        ///     Arguments passed to your command
        /// </param>
        public abstract void RunCommand(string[] args);

        public abstract bool VerifyArgs(string[] args);

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Description)}: {Description}, {nameof(Args)}: {Args}, {nameof(Alias)}: [{string.Join(", ", Alias)}]";
        }
    }
}

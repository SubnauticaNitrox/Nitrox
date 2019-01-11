using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;

namespace NitroxServer.ConsoleCommands.Abstract
{
    public abstract class Command
    {
        public Optional<string> Args { get; protected set; }
        public string Name { get; protected set; }
        public string[] Alias { get; protected set; }
        public string Description { get; protected set; }
        public Perms RequiredPermLevel { get; protected set; } = Perms.Admin;

        protected Command(string name, Perms requiredPermLevel) : this(name, requiredPermLevel, Optional<string>.Empty(), "", null)
        {
            RequiredPermLevel = requiredPermLevel;
            Name = name;
        }

        protected Command(string name, Perms requiredPermLevel, Optional<string> args) : this(name, requiredPermLevel, args, "", null)
        {
            Args = args;
            Name = name;
        }

        protected Command(string name, Perms requiredPermLevel, Optional<string> args, string description) : this(name, requiredPermLevel, args, "", null)
        {
            Args = args;
            Name = name;
            Description = description;
        }

        protected Command(string name, Perms requiredPermLevel, Optional<string> args, string description, string[] alias)
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

        public virtual void RunCommand(string[] args, Optional<Player> player)
        {
            if (player.IsEmpty())
            {
                Log.Info("Unimplemented command!");
            }
            else
            {
                Log.Info("Unimplemented player command!");
            }
        }

        public abstract bool VerifyArgs(string[] args);

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Description)}: {Description}, {nameof(Args)}: {Args}, {nameof(Alias)}: [{string.Join(", ", Alias)}]";
        }
    }
}

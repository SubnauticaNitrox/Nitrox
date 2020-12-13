using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Helper;
using Nitrox.Server.ConsoleCommands.Abstract;
using Nitrox.Server.ConsoleCommands.Abstract.Type;

namespace Nitrox.Server.ConsoleCommands
{
    internal class WarpCommand : Command
    {
        public WarpCommand() : base("warp", Perms.ADMIN, "Allows to teleport players")
        {
            AddParameter(new TypePlayer("name", true));
            AddParameter(new TypePlayer("name", false));
        }

        protected override void Execute(CallArgs args)
        {
            Player destination;
            Player sender;

            if (args.IsValid(1))
            {
                destination = args.Get<Player>(1);
                sender = args.Get<Player>(0);
            }
            else
            {
                Validate.IsTrue(args.Sender.HasValue, "This command can't be used by CONSOLE");
                destination = args.Get<Player>(0);
                sender = args.Sender.Value;
            }

            sender.Teleport(destination.Position);
            SendMessage(sender, $"Teleported to {destination.Name}");
        }
    }
}

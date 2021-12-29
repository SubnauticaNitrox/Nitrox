using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class WarpCommand : Command
    {
        public WarpCommand() : base("warp", Perms.MODERATOR, "Allows to teleport players")
        {
            AddParameter(new TypePlayer("name", true, "Player to teleport to (or a player specified to teleport)"));
            AddParameter(new TypePlayer("name", false, "The players name to teleport to"));
        }

        protected override void Execute(CallArgs args)
        {
            Player destination;
            Player sender;

            //Allows the console to teleport two players
            if (args.IsValid(1))
            {
                destination = args.Get<Player>(1);
                sender = args.Get<Player>(0);
            }
            else
            {
                Validate.IsFalse(args.IsConsole, "This command can't be used by CONSOLE");
                destination = args.Get<Player>(0);
                sender = args.Sender.Value;
            }

            sender.Teleport(destination.Position, destination.SubRootId);
            SendMessage(sender, $"Teleported to {destination.Name}");
        }
    }
}

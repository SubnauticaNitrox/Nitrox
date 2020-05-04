using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class BackCommand : Command
    {
        public BackCommand() : base("back", Perms.ADMIN, "Teleports you back on your last location")
        {
        }

        protected override void Execute(CallArgs args)
        {
            Validate.IsTrue(args.Sender.HasValue, "This command can't be used by CONSOLE");

            Player player = args.Sender.Value;

            Validate.IsTrue(player.LastTeleportationPosition.HasValue, "No previous location...");
            player.Teleport(player.LastTeleportationPosition.Value);

            SendMessage(args.Sender, $"Teleported back {player.LastTeleportationPosition.Value}");
        }
    }
}

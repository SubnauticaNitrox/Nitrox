using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxServer.ConsoleCommands.Abstract;

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

            Validate.IsTrue(player.LastStoredPosition.HasValue, "No previous location...");
            player.Teleport(player.LastStoredPosition.Value);

            SendMessage(args.Sender, $"Teleported back {player.LastStoredPosition.Value}");
        }
    }
}

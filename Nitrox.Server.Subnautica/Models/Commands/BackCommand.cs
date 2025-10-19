using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class BackCommand : Command
    {
        public BackCommand() : base("back", Perms.MODERATOR, PermsFlag.NO_CONSOLE, "Teleports you back on your last location")
        {
        }

        protected override void Execute(CallArgs args)
        {
            Player player = args.Sender.Value;

            if (player.LastStoredPosition == null)
            {
                SendMessage(args.Sender, "No previous location...");
                return;
            }

            player.Teleport(player.LastStoredPosition.Value, player.LastStoredSubRootID);
            SendMessage(args.Sender, $"Teleported back to {player.LastStoredPosition.Value}");
        }
    }
}

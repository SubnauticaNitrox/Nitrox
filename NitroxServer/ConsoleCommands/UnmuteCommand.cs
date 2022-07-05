using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class UnmuteCommand : Command
    {
        private readonly PlayerManager playerManager;

        public UnmuteCommand(PlayerManager playerManager) : base("unmute", Perms.MODERATOR, "Removes a mute from a player")
        {
            this.playerManager = playerManager;
            AddParameter(new TypePlayer("name", true, "Player to unmute"));
        }

        protected override void Execute(CallArgs args)
        {
            Player targetPlayer = args.Get<Player>(0);

            if (args.SenderName == targetPlayer.Name)
            {
                SendMessage(args.Sender, "You can't unmute yourself");
                return;
            }

            if (!args.IsConsole && targetPlayer.Permissions >= args.Sender.Value.Permissions)
            {
                SendMessage(args.Sender, $"You're not allowed to unmute {targetPlayer.Name}");
                return;
            }

            if (!targetPlayer.IsMuted)
            {
                SendMessage(args.Sender, $"{targetPlayer.Name} is already unmuted");
                return;
            }

            targetPlayer.IsMuted = false;
            // TODO: Send a packet to all players to acknowledge the unmuted player
            SendMessage(targetPlayer, "You're no longer muted");
            SendMessage(args.Sender, $"Unmuted {targetPlayer.Name}");
        }
    }
}

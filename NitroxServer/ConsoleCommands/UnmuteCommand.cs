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

            if (!targetPlayer.PlayerContext.IsMuted)
            {
                SendMessage(args.Sender, $"{targetPlayer.Name} is already unmuted");
                args.Sender.Value.SendPacket(new MutePlayer(targetPlayer.Id, targetPlayer.PlayerContext.IsMuted));
                return;
            }

            targetPlayer.PlayerContext.IsMuted = false;
            playerManager.SendPacketToAllPlayers(new MutePlayer(targetPlayer.Id, targetPlayer.PlayerContext.IsMuted));
            SendMessage(targetPlayer, "You're no longer muted");
            SendMessage(args.Sender, $"Unmuted {targetPlayer.Name}");
        }
    }
}

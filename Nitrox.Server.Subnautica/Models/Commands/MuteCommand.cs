using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class MuteCommand : Command
    {
        private readonly PlayerManager playerManager;

        public MuteCommand(PlayerManager playerManager) : base("mute", Perms.MODERATOR, "Prevents a user from chatting")
        {
            this.playerManager = playerManager;
            AddParameter(new TypePlayer("name", true, "Player to mute"));
        }

        protected override void Execute(CallArgs args)
        {
            Player targetPlayer = args.Get<Player>(0);

            if (args.SenderName == targetPlayer.Name)
            {
                SendMessage(args.Sender, "You can't mute yourself");
                return;
            }

            if (!args.IsConsole && targetPlayer.Permissions >= args.Sender.Value.Permissions)
            {
                SendMessage(args.Sender, $"You're not allowed to mute {targetPlayer.Name}");
                return;
            }

            if (targetPlayer.PlayerContext.IsMuted)
            {
                SendMessage(args.Sender, $"{targetPlayer.Name} is already muted");
                args.Sender.Value.SendPacket(new MutePlayer(targetPlayer.Id, targetPlayer.PlayerContext.IsMuted));
                return;
            }

            targetPlayer.PlayerContext.IsMuted = true;
            playerManager.SendPacketToAllPlayers(new MutePlayer(targetPlayer.Id, targetPlayer.PlayerContext.IsMuted));
            SendMessage(targetPlayer, "You're now muted");
            SendMessage(args.Sender, $"Muted {targetPlayer.Name}");
        }
    }
}

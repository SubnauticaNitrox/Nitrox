using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands
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

            if (targetPlayer.IsMuted)
            {
                SendMessage(args.Sender, $"{targetPlayer.Name} is already muted");
                return;
            }

            targetPlayer.IsMuted = true;
            // TODO: Send a packet to all players to acknowledge the muted player
            SendMessage(targetPlayer, "You're now muted");
            SendMessage(args.Sender, $"Muted {targetPlayer.Name}");
        }
    }
}

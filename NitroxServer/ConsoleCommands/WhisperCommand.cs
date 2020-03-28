using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using NitroxModel.Packets;
using NitroxModel.DataStructures.Util;

namespace NitroxServer.ConsoleCommands
{
    internal class WhisperCommand : Command
    {
        private readonly PlayerManager playerManager;

        public WhisperCommand(PlayerManager playerManager) : base("msg", Perms.PLAYER, "{name} {msg}", "Sends a private message to a player", new string[] { "m", "whisper", "w" })
        {
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args, Optional<Player> sender)
        {
            Player foundPlayer;

            if (playerManager.TryGetPlayerByName(args[0], out foundPlayer))
            {
                string message = string.Join(" ", args.Skip(1).ToArray());

                if (sender.HasValue)
                {
                    foundPlayer.SendPacket(new ChatMessage(sender.Value.Id, message));
                }
                else
                {
                    foundPlayer.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, message));
                }
            }
            else
            {
                Notify(sender, $"Unable to whisper {args[0]}, player not found.");
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 2;
        }
    }
}

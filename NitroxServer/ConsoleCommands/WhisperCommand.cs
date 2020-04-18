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

        public WhisperCommand(PlayerManager playerManager) : base("msg", Perms.PLAYER, "Sends a private message to a player", true)
        {
            this.playerManager = playerManager;
            addAlias("m", "whisper", "w");
            addParameter(null, TypePlayer.Get, "name", true);
            addParameter(string.Empty, TypeString.Get, "msg", true);
        }

        public override void Perform(string[] args, Optional<Player> sender)
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
                SendMessageToBoth(sender, $"Unable to whisper {args[0]}, player not found.");
            }
        }
    }
}

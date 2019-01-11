using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using NitroxModel.Packets;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;

namespace NitroxServer.ConsoleCommands
{
    class WhisperCommand : Command
    {
        private readonly PlayerManager playerManager;

        public WhisperCommand(PlayerManager playerManager) : base("w", Perms.PLAYER, "w <PlayerName> <msg>")
        {
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args, Optional<Player> player)
        {
            Player foundPlayer;

            if (playerManager.TryGetPlayerByName(args[0], out foundPlayer))
            {
                args = args.Skip(1).ToArray();

                string message = string.Join(" ", args);

                if (player.IsPresent())
                {
                    foundPlayer.SendPacket(new ChatMessage(player.Get().Id, message));
                }
                else
                {
                    foundPlayer.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, message));
                }
            }
            else
            {
                string errorMessage = "Unable to whisper " + args[0] + " - player not found.";

                SendServerMessageIfPlayerIsPresent(player, errorMessage);
                Log.Info(errorMessage);
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 2;
        }
    }
}

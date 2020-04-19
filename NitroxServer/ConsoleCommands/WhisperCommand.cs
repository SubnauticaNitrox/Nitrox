using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.Util;

namespace NitroxServer.ConsoleCommands
{
    internal class WhisperCommand : Command
    {
        public WhisperCommand() : base("msg", Perms.PLAYER, "Sends a private message to a player", true)
        {
            addAlias("m", "whisper", "w");
            addParameter(TypePlayer.Get, "name", true);
            addParameter(TypeString.Get, "msg", true);
        }

        protected override void Perform(Optional<Player> sender)
        {
            Player foundPlayer = readArgAt(0);

            if (foundPlayer != null)
            {
                string message = $"[{GetSenderName(sender)} -> YOU]: {getArgOverflow(-1)}";
                SendMessage(foundPlayer, message);
            }
            else
            {
                SendMessage(sender, $"Unable to whisper, player not found.");
            }
        }
    }
}

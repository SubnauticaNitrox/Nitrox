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
            AddAlias("m", "whisper", "w");
            AddParameter(TypePlayer.Get, "name", true);
            AddParameter(TypeString.Get, "msg", true);
        }

        protected override void Perform(Optional<Player> sender)
        {
            Player foundPlayer = ReadArgAt(0);

            if (foundPlayer != null)
            {
                string message = $"[{GetSenderName(sender)} -> YOU]: {GetArgOverflow(-1)}";
                SendMessage(foundPlayer, message);
            }
            else
            {
                SendMessage(sender, $"Unable to whisper, player not found.");
            }
        }
    }
}

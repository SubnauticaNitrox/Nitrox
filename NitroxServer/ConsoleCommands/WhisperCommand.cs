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

        public override void Perform(Optional<Player> sender)
        {
            Player foundPlayer = readArgAt(0);

            if (foundPlayer != null)
            {
                string message = string.Format("[{0} -> YOU]: {1}",
                    GetSenderName(sender),
                    string.Join(" ", Args.Skip(1).ToArray())
                    );

                SendMessage(foundPlayer, message);
            }
            else
            {
                SendMessage(sender, $"Unable to whisper, player not found.");
            }
        }
    }
}

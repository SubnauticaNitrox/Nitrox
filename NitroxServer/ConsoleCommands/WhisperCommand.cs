using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class WhisperCommand : Command
    {
        public WhisperCommand() : base("msg", Perms.PLAYER, "Sends a private message to a player", true)
        {
            AddAlias("m", "whisper", "w");
            AddParameter(new TypePlayer("name", true));
            AddParameter(new TypeString("msg", true));
        }

        protected override void Execute(Optional<Player> sender)
        {
            Player foundPlayer = ReadArgAt<Player>(0);

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

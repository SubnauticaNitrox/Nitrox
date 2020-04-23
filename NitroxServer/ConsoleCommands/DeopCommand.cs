using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class DeopCommand : Command
    {
        public DeopCommand() : base("deop", Perms.ADMIN, "Removes admin rights from user")
        {
            AddParameter(new TypePlayer("name", true));
        }

        protected override void Execute(Optional<Player> sender)
        {
            Player targetPlayer = ReadArgAt<Player>(0);
            string playerName = ReadArgAt(0);

            targetPlayer.Permissions = Perms.PLAYER;

            SendMessage(sender, $"Updated {playerName}\'s permissions to PLAYER");
        }
    }
}

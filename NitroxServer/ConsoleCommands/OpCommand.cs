using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class OpCommand : Command
    {
        public OpCommand() : base("op", Perms.ADMIN, "Sets an user as admin")
        {
            AddParameter(new TypePlayer("name", true));
        }

        protected override void Perform(Optional<Player> sender)
        {
            Player receivingPlayer = ReadArgAt<Player>(0);
            string playerName = receivingPlayer.Name;

            receivingPlayer.Permissions = Perms.ADMIN;
            string message = $"Updated {playerName}\'s permissions to admin";

            SendMessageToBoth(sender, message);
        }
    }
}

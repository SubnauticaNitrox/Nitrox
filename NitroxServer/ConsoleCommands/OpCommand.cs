using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract;

namespace NitroxServer.ConsoleCommands
{
    internal class OpCommand : Command
    {
        public OpCommand() : base("op", Perms.ADMIN, "Sets an user as admin")
        {
            addParameter(TypePlayer.Get, "name", true);
        }

        public override void Perform(Optional<Player> sender)
        {
            Player receivingPlayer = readArgAt(0);
            string playerName = getArgAt(0);
            string message;

            if (receivingPlayer != null)
            {
                receivingPlayer.Permissions = Perms.ADMIN;
                message = $"Updated {playerName}\'s permissions to admin";
            }
            else
            {
                message = $"Could not update permissions of unknown player '{playerName}'";
            }

            SendMessageToBoth(sender, message);
        }
    }
}

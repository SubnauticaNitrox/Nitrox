using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;

namespace NitroxServer.ConsoleCommands
{
    internal class SaveCommand : Command
    {
        public SaveCommand() : base("save", Perms.ADMIN, "Saves the map")
        {
        }

        protected override void Execute(Optional<Player> sender)
        {
            Server.Instance.Save();
            SendMessageToPlayer(sender, "World saved");
        }
    }
}

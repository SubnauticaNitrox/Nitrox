using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;

namespace NitroxServer.ConsoleCommands
{
    internal class SaveCommand : Command
    {
        public SaveCommand() : base("save", Perms.ADMIN, "Saves the map")
        {
        }

        protected override void Execute(CallArgs args)
        {
            Server.Instance.Save();
            SendMessageToPlayer(args.Sender, "World saved");
        }
    }
}

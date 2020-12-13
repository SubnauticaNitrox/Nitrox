using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.ConsoleCommands.Abstract;

namespace Nitrox.Server.ConsoleCommands
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

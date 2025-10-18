using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class SaveCommand : Command
    {
        public SaveCommand() : base("save", Perms.MODERATOR, "Saves the map")
        {
        }

        protected override void Execute(CallArgs args)
        {
            Server.Instance.Save();
            SendMessageToPlayer(args.Sender, "World saved");
        }
    }
}

using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class SaveCommand : Command
    {
        private readonly SaveService saveService;

        public SaveCommand(SaveService saveService) : base("save", Perms.MODERATOR, "Saves the map")
        {
            this.saveService = saveService;
        }

        protected override void Execute(CallArgs args)
        {
            saveService.QueueSave();
            SendMessageToPlayer(args.Sender, "Saving world...");
        }
    }
}

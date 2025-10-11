using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class BackupCommand : Command
    {
        public BackupCommand() : base("backup", Perms.ADMIN, "Creates a backup of the save")
        {
        }
    
        protected override void Execute(CallArgs args)
        {
            Server.Instance.BackUp();
            SendMessageToPlayer(args.Sender, "World has been backed up");
        }
    }
}

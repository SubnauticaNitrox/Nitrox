using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class DeopCommand : Command
    {
        public DeopCommand() : base("deop", Perms.ADMIN, "Removes admin rights from user")
        {
            AddParameter(new TypePlayer("name", true, "Username to remove admin rights from"));
        }

        protected override void Execute(CallArgs args)
        {
            Player targetPlayer = args.Get<Player>(0);
            targetPlayer.Permissions = Perms.PLAYER;

            // Need to notify him so that he no longer shows admin stuff on client (which would in any way stop working)
            targetPlayer.SendPacket(new PermsChanged(targetPlayer.Permissions));
            SendMessage(targetPlayer, "You were demoted to PLAYER");
            SendMessage(args.Sender, $"Updated {targetPlayer.Name}\'s permissions to PLAYER");
        }
    }
}

using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class OpCommand : Command
    {
        public OpCommand() : base("op", Perms.ADMIN, "Sets a user as admin")
        {
            AddParameter(new TypePlayer("name", true, "The players name to make an admin"));
        }

        protected override void Execute(CallArgs args)
        {
            Player targetPlayer = args.Get<Player>(0);
            targetPlayer.Permissions = Perms.ADMIN;

            // We need to notify this player that he can show all the admin-related stuff
            targetPlayer.SendPacket(new PermsChanged(targetPlayer.Permissions));
            SendMessage(targetPlayer, "You were promoted to ADMIN");
            SendMessage(args.Sender, $"Updated {targetPlayer.Name}\'s permissions to ADMIN");
        }
    }
}

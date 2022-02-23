using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class PromoteCommand : Command
    {
        public PromoteCommand() : base("promote", Perms.MODERATOR, "Sets specific permissions to a user")
        {
            AddParameter(new TypePlayer("name", true, "The username to change the permissions of"));
            AddParameter(new TypeEnum<Perms>("perms", true, "Permission level"));
        }

        protected override void Execute(CallArgs args)
        {
            Player targetPlayer = args.Get<Player>(0);
            Perms permissions = args.Get<Perms>(1);

            if (args.SenderName == targetPlayer.Name)
            {
                SendMessage(args.Sender, "You can't promote yourself");
                return;
            }

            //Allows a bounded permission hierarchy
            if (args.IsConsole || permissions < args.Sender.Value.Permissions)
            {
                targetPlayer.Permissions = permissions;

                targetPlayer.SendPacket(new PermsChanged(targetPlayer.Permissions));
                SendMessage(args.Sender, $"Updated {targetPlayer.Name}\'s permissions to {permissions}");
                SendMessageToPlayer(targetPlayer, $"You've been promoted to {permissions}");
            }
            else
            {
                SendMessage(args.Sender, $"You're not allowed to update {targetPlayer.Name}\'s permissions");
            }
        }
    }
}

using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
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

            SendMessage(args.Sender, $"Updated {targetPlayer.Name}\'s permissions to ADMIN");
        }
    }
}

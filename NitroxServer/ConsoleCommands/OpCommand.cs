using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class OpCommand : Command
    {
        public OpCommand() : base("op", Perms.ADMIN, "Sets an user as admin")
        {
            AddParameter(new TypePlayer("name", true));
        }

        protected override void Execute(CallArgs args)
        {
            Player receivingPlayer = args.Get<Player>(0);
            string playerName = receivingPlayer.Name;

            receivingPlayer.Permissions = Perms.ADMIN;
            SendMessage(args.Sender, $"Updated {playerName}\'s permissions to admin");
        }
    }
}

using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;

namespace NitroxServer.ConsoleCommands
{
    internal class SummaryCommand : Command
    {
        private readonly Server server;

        public SummaryCommand(Server server) : base("summary", Perms.PLAYER, "Shows persisted data", true)
        {
            this.server = server;
        }

        protected override void Execute(CallArgs args)
        {
            SendMessage(args.Sender, server.SaveSummary);
        }
    }
}

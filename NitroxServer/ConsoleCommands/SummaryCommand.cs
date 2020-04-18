using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;

namespace NitroxServer.ConsoleCommands
{
    internal class SummaryCommand : Command
    {
        private readonly Server server;

        public SummaryCommand(Server server) : base("summary", Perms.PLAYER, "Shows persisted data")
        {
            this.server = server;
        }

        public override void Perform(string[] args, Optional<Player> sender)
        {
            SendMessage(sender, server.SaveSummary);
        }
    }
}

using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;

namespace NitroxServer.ConsoleCommands
{
    internal class SummaryCommand : Command
    {
        private readonly Server server;

        public SummaryCommand(Server server) : base("summary", Perms.PLAYER, "", "Shows persisted data")
        {
            this.server = server;
        }

        public override void RunCommand(string[] args, Optional<Player> sender)
        {
            if (sender.HasValue)
            {
                SendMessageToPlayer(sender, server.SaveSummary);
            }
            else
            {
                Log.Info(server.SaveSummary);
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 0;
        }
    }
}

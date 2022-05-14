using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;

namespace NitroxServer.ConsoleCommands
{
    internal class SummaryCommand : Command
    {
        private readonly Server server;

        public SummaryCommand(Server server) : base("summary", Perms.MODERATOR, "Shows persisted data")
        {
            this.server = server;

            AllowedArgOverflow = true;
        }

        protected override void Execute(CallArgs args)
        {
            Perms viewerPerms = args.Sender.OrElse(null)?.Permissions ?? Perms.PLAYER;
            SendMessage(args.Sender, server.GetSaveSummary(viewerPerms));
        }
    }
}

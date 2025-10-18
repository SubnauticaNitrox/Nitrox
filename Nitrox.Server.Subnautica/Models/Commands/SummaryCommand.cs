using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;

namespace Nitrox.Server.Subnautica.Models.Commands
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
            Perms viewerPerms = args.Sender.OrNull()?.Permissions ?? Perms.PLAYER;
            SendMessage(args.Sender, server.GetSaveSummary(viewerPerms));
        }
    }
}

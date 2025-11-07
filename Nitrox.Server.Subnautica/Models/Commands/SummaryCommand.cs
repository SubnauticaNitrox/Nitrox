using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class SummaryCommand : Command
    {
        private readonly StatusService statusService;

        public SummaryCommand(StatusService statusService) : base("summary", Perms.MODERATOR, "Shows persisted data")
        {
            this.statusService = statusService;

            AllowedArgOverflow = true;
        }

        protected override void Execute(CallArgs args)
        {
            Perms viewerPerms = args.Sender.OrNull()?.Permissions ?? Perms.HOST;
            SendMessage(args.Sender, statusService.GetServerSummary(viewerPerms).GetAwaiter().GetResult());
        }
    }
}

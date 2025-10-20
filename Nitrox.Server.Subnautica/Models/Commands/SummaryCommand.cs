using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class SummaryCommand : Command
    {
        private readonly StatusService statusService;
        private readonly ILogger<SummaryCommand> logger;

        public SummaryCommand(StatusService statusService, ILogger<SummaryCommand> logger) : base("summary", Perms.MODERATOR, "Shows persisted data")
        {
            this.statusService = statusService;
            this.logger = logger;

            AllowedArgOverflow = true;
        }

        protected override void Execute(CallArgs args)
        {
            Perms viewerPerms = args.Sender.OrNull()?.Permissions ?? Perms.HOST;
            // TODO: Make command execute async
            string senderName = args.SenderName;
            Task.Run(async () =>
            {
                using (logger.BeginOutputToPeerScope(senderName))
                {
                    await statusService.SummarizeServer(viewerPerms);
                }
            }).ContinueWithHandleError();
        }
    }
}

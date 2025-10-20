using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Logging.Scopes;
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
            Player? sender = args.Sender.OrNull();
            // TODO: Make command execute async
            Task.Run(async () =>
            {
                using CaptureScope scope = logger.BeginCaptureScope();
                await statusService.LogServerSummary(viewerPerms);
                SendMessage(sender, string.Join("", scope.Logs));
            }).ContinueWithHandleError();
        }
    }
}

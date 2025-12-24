using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class ChangeAdminPasswordCommand : Command
    {
        private readonly IOptions<SubnauticaServerOptions> options;
        private readonly ILogger<ChangeAdminPasswordCommand> logger;

        public ChangeAdminPasswordCommand(IOptions<SubnauticaServerOptions> options, ILogger<ChangeAdminPasswordCommand> logger) : base("changeadminpassword", Perms.ADMIN, "Changes admin password")
        {
            AddParameter(new TypeString("password", true, "The new admin password"));

            this.options = options;
            this.logger = logger;
        }

        protected override void Execute(CallArgs args)
        {
            string newPassword = args.Get(0);
            options.Value.AdminPassword = newPassword;
            logger.ZLogInformation($"Admin password changed to {newPassword:@password} by {args.SenderName:@playername}");

            SendMessageToPlayer(args.Sender, "Admin password has been updated");
        }
    }
}

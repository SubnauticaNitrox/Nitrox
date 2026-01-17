using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class ChangeServerPasswordCommand : Command
    {
        private readonly IOptions<SubnauticaServerOptions> serverConfig;
        private readonly ILogger<ChangeServerPasswordCommand> logger;

        public ChangeServerPasswordCommand(IOptions<SubnauticaServerOptions> serverConfig, ILogger<ChangeServerPasswordCommand> logger) : base("changeserverpassword", Perms.ADMIN, "Changes server password. Clear it without argument")
        {
            AddParameter(new TypeString("password", false, "The new server password"));

            this.serverConfig = serverConfig;
            this.logger = logger;
        }

        protected override void Execute(CallArgs args)
        {
            string password = args.Get(0) ?? string.Empty;

            serverConfig.Value.ServerPassword = password;

            logger.LogServerPasswordChanged(password, args.SenderName);
            SendMessageToPlayer(args.Sender, "Server password has been updated");
        }
    }
}

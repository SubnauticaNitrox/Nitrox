using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Server;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class ChangeAdminPasswordCommand : Command
    {
        private readonly ServerConfig serverConfig;

        public ChangeAdminPasswordCommand(ServerConfig serverConfig) : base("changeadminpassword", Perms.ADMIN, "Changes admin password")
        {
            this.serverConfig = serverConfig;
            AddParameter(new TypeString("password", true));
        }

        protected override void Execute(CallArgs args)
        {
            string newPassword = args.Get(0);

            serverConfig.AdminPassword = newPassword;

            Log.Info($"Admin password changed to \"{newPassword}\" by {args.SenderName}");
            SendMessageToPlayer(args.Sender, "Admin password changed");
        }
    }
}

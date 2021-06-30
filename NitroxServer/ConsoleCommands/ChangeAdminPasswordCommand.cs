using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.Serialization;

namespace NitroxServer.ConsoleCommands
{
    internal class ChangeAdminPasswordCommand : Command
    {
        private readonly ServerConfig serverConfig;

        public ChangeAdminPasswordCommand(ServerConfig serverConfig) : base("changeadminpassword", Perms.ADMIN, "Changes admin password")
        {
            AddParameter(new TypeString("password", true));

            this.serverConfig = serverConfig;
        }

        protected override void Execute(CallArgs args)
        {
            string newPassword = args.Get(0);

            serverConfig.AdminPassword = newPassword;
            serverConfig.Serialize();

            Log.InfoSensitive("Admin password changed to {password} by {playername}", newPassword, args.SenderName);
            SendMessageToPlayer(args.Sender, "Admin password has been updated");
        }
    }
}

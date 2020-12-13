using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Logger;
using Nitrox.Server.ConsoleCommands.Abstract;
using Nitrox.Server.ConsoleCommands.Abstract.Type;
using Nitrox.Server.Serialization;

namespace Nitrox.Server.ConsoleCommands
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

            Log.InfoSensitive("Admin password changed to {password} by {playername}", newPassword, args.SenderName);
            SendMessageToPlayer(args.Sender, "Admin password changed");
        }
    }
}

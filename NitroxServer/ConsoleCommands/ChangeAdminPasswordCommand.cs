using System.IO;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Serialization;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class ChangeAdminPasswordCommand : Command
    {
        private readonly Server server;
        private readonly SubnauticaServerConfig serverConfig;

        public ChangeAdminPasswordCommand(Server server, SubnauticaServerConfig serverConfig) : base("changeadminpassword", Perms.ADMIN, "Changes admin password")
        {
            AddParameter(new TypeString("password", true, "The new admin password"));

            this.server = server;
            this.serverConfig = serverConfig;
        }

        protected override void Execute(CallArgs args)
        {
            string saveDir = Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), server.Name);
            using (serverConfig.Update(saveDir))
            {
                string newPassword = args.Get(0);
                serverConfig.AdminPassword = newPassword;
                Log.InfoSensitive("Admin password changed to {password} by {playername}", newPassword, args.SenderName);
            }

            SendMessageToPlayer(args.Sender, "Admin password has been updated");
        }
    }
}

using System.IO;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Serialization;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.Serialization.World;

namespace NitroxServer.ConsoleCommands
{
    internal class ChangeAdminPasswordCommand : Command
    {
        private readonly SubnauticaServerConfig serverConfig;

        public ChangeAdminPasswordCommand(SubnauticaServerConfig serverConfig) : base("changeadminpassword", Perms.ADMIN, "Changes admin password")
        {
            AddParameter(new TypeString("password", true, "The new admin password"));

            this.serverConfig = serverConfig;
        }

        protected override void Execute(CallArgs args)
        {
            string saveDir = Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), serverConfig.SaveName);
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

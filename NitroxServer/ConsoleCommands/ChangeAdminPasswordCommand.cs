using System;
using System.IO;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.Serialization;
using NitroxServer.Serialization.World;

namespace NitroxServer.ConsoleCommands
{
    internal class ChangeAdminPasswordCommand : Command
    {
        private readonly ServerConfig serverConfig;

        public ChangeAdminPasswordCommand(ServerConfig serverConfig) : base("changeadminpassword", Perms.ADMIN, "Changes admin password")
        {
            AddParameter(new TypeString("password", true, "The new admin password"));

            this.serverConfig = serverConfig;
        }

        protected override void Execute(CallArgs args)
        {
            serverConfig.Update(Path.Combine(WorldManager.SavesFolderDir, serverConfig.SaveName), c =>
            {
                string newPassword = args.Get(0);
                c.AdminPassword = newPassword;
                Log.InfoSensitive("Admin password changed to {password} by {playername}", newPassword, args.SenderName);
            });

            SendMessageToPlayer(args.Sender, "Admin password has been updated");
        }
    }
}

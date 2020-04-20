using System;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Server;
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

        protected override void Execute(Optional<Player> sender)
        {
            string playerName = GetSenderName(sender);
            string newPassword = ReadArgAt(0);
            serverConfig.AdminPassword = newPassword;

            Log.Info($"Admin password changed to \"{newPassword}\" by {playerName}");
            SendMessageToPlayer(sender, "Admin password changed");
        }
    }
}

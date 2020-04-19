using System;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Server;

namespace NitroxServer.ConsoleCommands
{
    internal class ChangeAdminPasswordCommand : Command
    {
        private readonly ServerConfig serverConfig;

        public ChangeAdminPasswordCommand(ServerConfig serverConfig) : base("changeadminpassword", Perms.ADMIN, "Changes admin password")
        {
            this.serverConfig = serverConfig;
            AddParameter(TypeString.Get, "password", true);
        }

        protected override void Perform(Optional<Player> sender)
        {
            string playerName = GetSenderName(sender);
            serverConfig.AdminPassword = GetArgAt(0);

            Log.Info($"Admin password changed to \"{GetArgAt(0)}\" by {playerName}");
            SendMessageToPlayer(sender, "Admin password changed");
        }
    }
}

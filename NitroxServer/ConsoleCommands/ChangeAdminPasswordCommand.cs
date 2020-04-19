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
            addParameter(TypeString.Get, "password", true);
        }

        public override void Perform(Optional<Player> sender)
        {
            try
            {
                string playerName = GetSenderName(sender);
                serverConfig.AdminPassword = getArgAt(0);

                Log.Info($"Admin password changed to \"{getArgAt(0)}\" by {playerName}");
                SendMessageToPlayer(sender, "Admin password changed");
            }
            catch (Exception ex)
            {
                Log.Error($"Error while attempting to change admin password", ex);
            }
        }
    }
}

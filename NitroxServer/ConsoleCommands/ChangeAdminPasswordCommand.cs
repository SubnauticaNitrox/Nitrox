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

        public ChangeAdminPasswordCommand(ServerConfig serverConfig) : base("changeadminpassword", Perms.ADMIN, "{password}", "Changes admin password")
        {
            this.serverConfig = serverConfig;
        }

        public override void RunCommand(string[] args, Optional<Player> sender)
        {
            try
            {
                string playerName = sender.HasValue ? sender.Value.Name : "SERVER";
                serverConfig.AdminPassword = args[0];

                Log.InfoSensitive("Admin password changed to {password} by {playername}", args[0], playerName);
                SendMessageToPlayer(sender, "Admin password changed");
            }
            catch (Exception ex)
            {
                Log.ErrorSensitive(ex, "Error attempting to change admin password to {password}", args[0]);
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length >= 1;
        }
    }
}

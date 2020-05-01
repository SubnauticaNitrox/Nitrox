using System;
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
            try
            {
                string playerName = args.Sender.HasValue ? args.Sender.Value.Name : "SERVER";
                serverConfig.AdminPassword = args.Args[0];

                Log.InfoSensitive("Admin password changed to {password} by {playername}", args.Args[0], playerName);
                SendMessageToPlayer(args.Sender, "Admin password changed");
            }
            catch (Exception ex)
            {
                Log.ErrorSensitive(ex, "Error attempting to change admin password to {password}", args.Args[0]);
            }
        }
    }
}

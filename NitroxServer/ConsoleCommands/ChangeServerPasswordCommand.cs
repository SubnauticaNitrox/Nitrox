using System;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Server;

namespace NitroxServer.ConsoleCommands
{
    internal class ChangeServerPasswordCommand : Command
    {
        private readonly ServerConfig serverConfig;

        public ChangeServerPasswordCommand(ServerConfig serverConfig) : base("changeserverpassword", Perms.ADMIN, "[{password}]", "Changes server password. Clear it without argument")
        {
            this.serverConfig = serverConfig;
        }

        public override void RunCommand(string[] args, Optional<Player> sender)
        {
            try
            {
                string playerName = sender.HasValue ? sender.Value.Name : "SERVER";
                string password = args.Length == 0 ? "" : args[0];
                serverConfig.ServerPassword = password;

                Log.Info($"Server password changed to \"{password}\" by {playerName}");
                SendMessageToPlayer(sender, "Server password changed");
            }
            catch (Exception ex)
            {
                Log.Error($"Error attempting to change server password", ex);
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length >= 0;
        }
    }
}

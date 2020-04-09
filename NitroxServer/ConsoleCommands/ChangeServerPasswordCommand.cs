using System;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConfigParser;

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
                serverConfig.ChangeServerPassword(password);

                Log.Info($"Server password changed to \"{password}\" by {playerName}");
                SendMessageToPlayer(sender, "Server password changed");
            }
            catch (Exception ex)
            {
                Log.Exception("Error attempting to change server password", ex);
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length >= 0;
        }

        private void ChangeServerPassword(string password, string name)
        {
            serverConfig.ChangeServerPassword(password);
            Log.InfoSensitive("Server password changed to \"{0}\" by \"{1}\"", password, name);
        }
    }
}

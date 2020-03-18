using System;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConfigParser;

namespace NitroxServer.ConsoleCommands
{
    internal class ChangeServerPasswordCommand : Command
    {
        private readonly ServerConfig serverConfig;

        public ChangeServerPasswordCommand(ServerConfig serverConfig) : base("changeserverpassword", Perms.ADMIN, "[<password>]", "Changes server password. Clear it without argument")
        {
            this.serverConfig = serverConfig;
        }

        public override void RunCommand(string[] args, Optional<Player> sender)
        {
            try
            {
                string playerName = sender.IsPresent() ? sender.Get().Name : "SERVER";
                serverConfig.ChangeServerPassword(args.Length == 0 ? string.Empty : args[0]);

                Log.Info($"Server password changed to \"{args[0]}\" by {playerName}");
                SendMessageToPlayer(sender, "Server password changed");
            }
            catch (Exception ex)
            {
                Log.Error($"Error attempting to change server password: {args[0]}", ex);
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length >= 0;
        }
    }
}

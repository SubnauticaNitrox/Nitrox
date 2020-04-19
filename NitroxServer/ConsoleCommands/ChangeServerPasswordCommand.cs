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

        public ChangeServerPasswordCommand(ServerConfig serverConfig) : base("changeserverpassword", Perms.ADMIN, "Changes server password. Clear it without argument")
        {
            this.serverConfig = serverConfig;
            addParameter(TypeString.Get, "password", false);
        }

        protected override void Perform(Optional<Player> sender)
        {
            string playerName = GetSenderName(sender);
            string password = getArgAt(0) ?? string.Empty;
            serverConfig.ServerPassword = password;

            Log.Info($"Server password changed to \"{password}\" by {playerName}");
            SendMessageToPlayer(sender, "Server password changed");
        }
    }
}

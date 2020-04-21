using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Server;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class ChangeServerPasswordCommand : Command
    {
        private readonly ServerConfig serverConfig;

        public ChangeServerPasswordCommand(ServerConfig serverConfig) : base("changeserverpassword", Perms.ADMIN, "Changes server password. Clear it without argument")
        {
            this.serverConfig = serverConfig;
            AddParameter(new TypeString("password", false));
        }

        protected override void Execute(Optional<Player> sender)
        {
            string playerName = GetSenderName(sender);
            string password = ReadArgAt(0) ?? string.Empty;

            serverConfig.ServerPassword = password;

            Log.Info($"Server password changed to \"{password}\" by {playerName}");
            SendMessageToPlayer(sender, "Server password changed");
        }
    }
}

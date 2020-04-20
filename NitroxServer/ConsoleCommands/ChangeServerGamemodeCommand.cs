using System;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Server;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class ChangeServerGamemodeCommand : Command
    {
        private readonly ServerConfig serverConfig;

        public ChangeServerGamemodeCommand(ServerConfig serverConfig) : base("changeservergamemode", Perms.CONSOLE, "Changes server gamemode")
        {
            this.serverConfig = serverConfig;
            AddParameter(new TypeEnum<ServerGameMode>("gamemode", true));
        }

        protected override void Perform(Optional<Player> sender)
        {
            ServerGameMode? sgm = ReadArgAt<ServerGameMode>(0);
            string name = GetSenderName(sender);

            serverConfig.GameModeEnum = sgm.Value;
            SendMessageToBoth(sender, $"Server gamemode changed to \"{sgm?.ToString()}\" by {name}");
        }
    }
}

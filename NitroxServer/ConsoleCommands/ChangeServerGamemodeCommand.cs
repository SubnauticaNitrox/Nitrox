using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel.Server;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class ChangeServerGamemodeCommand : Command
    {
        private readonly ServerConfig serverConfig;
        private readonly PlayerManager playerManager;

        public ChangeServerGamemodeCommand(ServerConfig serverConfig, PlayerManager playerManager) : base("changeservergamemode", Perms.ADMIN, "Changes server gamemode")
        {
            this.serverConfig = serverConfig;
            this.playerManager = playerManager;
            AddParameter(new TypeEnum<ServerGameMode>("gamemode", true));
        }

        protected override void Execute(CallArgs args)
        {
            ServerGameMode sgm = args.Get<ServerGameMode>(0);

            serverConfig.GameModeEnum = sgm;
            playerManager.SendPacketToAllPlayers(new GameModeChanged(sgm));

            SendMessageToAllPlayers($"Server gamemode changed to \"{sgm}\" by {args.SenderName}");
        }
    }
}

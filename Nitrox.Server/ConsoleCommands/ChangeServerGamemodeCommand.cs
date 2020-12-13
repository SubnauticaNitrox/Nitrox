using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Model.Server;
using Nitrox.Server.ConsoleCommands.Abstract;
using Nitrox.Server.ConsoleCommands.Abstract.Type;
using Nitrox.Server.GameLogic;
using Nitrox.Server.Serialization;

namespace Nitrox.Server.ConsoleCommands
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

            serverConfig.GameMode = sgm;
            playerManager.SendPacketToAllPlayers(new GameModeChanged(sgm));

            SendMessageToAllPlayers($"Server gamemode changed to \"{sgm}\" by {args.SenderName}");
        }
    }
}

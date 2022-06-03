using System;
using System.IO;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel.Server;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;
using NitroxServer.Serialization;
using NitroxServer.Serialization.World;

namespace NitroxServer.ConsoleCommands
{
    internal class ChangeServerGamemodeCommand : Command
    {
        private readonly PlayerManager playerManager;
        private readonly ServerConfig serverConfig;

        public ChangeServerGamemodeCommand(PlayerManager playerManager, ServerConfig serverConfig) : base("changeservergamemode", Perms.ADMIN, "Changes server gamemode")
        {
            AddParameter(new TypeEnum<ServerGameMode>("gamemode", true, "Gamemode to change to"));

            this.playerManager = playerManager;
            this.serverConfig = serverConfig;
        }

        protected override void Execute(CallArgs args)
        {
            ServerGameMode sgm = args.Get<ServerGameMode>(0);

            serverConfig.Update(Path.Combine(WorldManager.SavesFolderDir, serverConfig.SaveName), c =>
            {
                if (c.GameMode != sgm)
                {
                    c.GameMode = sgm;

                    playerManager.SendPacketToAllPlayers(new GameModeChanged(sgm));
                    SendMessageToAllPlayers($"Server gamemode changed to \"{sgm}\" by {args.SenderName}");
                }
                else
                {
                    SendMessage(args.Sender, "Server is already using this gamemode");
                }
            });
        }
    }
}

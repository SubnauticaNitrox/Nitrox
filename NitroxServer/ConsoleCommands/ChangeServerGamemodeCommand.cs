using System.IO;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel.Server;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;
using NitroxServer.Serialization;
using NitroxServer.Serialization.World;

namespace NitroxServer.ConsoleCommands;

internal class ChangeServerGamemodeCommand : Command
{
    private readonly PlayerManager playerManager;
    private readonly ServerConfig serverConfig;

    public ChangeServerGamemodeCommand(PlayerManager playerManager, ServerConfig serverConfig) : base("changeservergamemode", Perms.ADMIN, "Changes server gamemode")
    {
        AddParameter(new TypeEnum<NitroxGameMode>("gamemode", true, "Gamemode to change to"));

        this.playerManager = playerManager;
        this.serverConfig = serverConfig;
    }

    protected override void Execute(CallArgs args)
    {
        NitroxGameMode sgm = args.Get<NitroxGameMode>(0);

        using (serverConfig.Update(Path.Combine(WorldManager.SavesFolderDir, serverConfig.SaveName)))
        {
            if (serverConfig.GameMode != sgm)
            {
                serverConfig.GameMode = sgm;

                foreach (Player player in playerManager.GetAllPlayers())
                {
                    player.GameMode = sgm;
                }
                playerManager.SendPacketToAllPlayers(GameModeChanged.ForAllPlayers(sgm));
                SendMessageToAllPlayers($"Server gamemode changed to \"{sgm}\" by {args.SenderName}");
            }
            else
            {
                SendMessage(args.Sender, "Server is already using this gamemode");
            }
        }
    }
}

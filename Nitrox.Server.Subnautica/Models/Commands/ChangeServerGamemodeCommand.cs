using System.IO;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Serialization;
using Nitrox.Model.Server;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

internal class ChangeServerGamemodeCommand : Command
{
    private readonly Server server;
    private readonly PlayerManager playerManager;
    private readonly SubnauticaServerConfig serverConfig;

    public ChangeServerGamemodeCommand(Server server, PlayerManager playerManager, SubnauticaServerConfig serverConfig) : base("changeservergamemode", Perms.ADMIN, "Changes server gamemode")
    {
        AddParameter(new TypeEnum<NitroxGameMode>("gamemode", true, "Gamemode to change to"));

        this.server = server;
        this.playerManager = playerManager;
        this.serverConfig = serverConfig;
    }

    protected override void Execute(CallArgs args)
    {
        NitroxGameMode sgm = args.Get<NitroxGameMode>(0);

        using (serverConfig.Update(Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), server.Name)))
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

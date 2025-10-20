using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

internal class ChangeServerGamemodeCommand : Command
{
    private readonly PlayerManager playerManager;
    private readonly IOptions<SubnauticaServerOptions> serverConfig;

    public ChangeServerGamemodeCommand(PlayerManager playerManager, IOptions<SubnauticaServerOptions> serverConfig) : base("changeservergamemode", Perms.ADMIN, "Changes server gamemode")
    {
        AddParameter(new TypeEnum<SubnauticaGameMode>("gamemode", true, "Gamemode to change to"));

        this.playerManager = playerManager;
        this.serverConfig = serverConfig;
    }

    protected override void Execute(CallArgs args)
    {
        SubnauticaGameMode sgm = args.Get<SubnauticaGameMode>(0);

        if (serverConfig.Value.GameMode != sgm)
        {
            serverConfig.Value.GameMode = sgm;

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

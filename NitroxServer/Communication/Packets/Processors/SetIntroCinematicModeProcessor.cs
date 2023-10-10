using System.Linq;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors;

public class SetIntroCinematicModeProcessor : AuthenticatedPacketProcessor<SetIntroCinematicMode>
{
    private readonly PlayerManager playerManager;

    public SetIntroCinematicModeProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public override void Process(SetIntroCinematicMode packet, Player player)
    {
        player.PlayerContext.IntroCinematicMode = packet.Mode;
        playerManager.SendPacketToOtherPlayers(packet, player);

        if (playerManager.GetAllPlayers().Count(p => p.PlayerContext?.IntroCinematicMode == SetIntroCinematicMode.IntroCinematicMode.WAITING) >= 2)
        {
            Log.Info("Starting Cinematic");

            Player[] pairedPlayers = playerManager.GetAllPlayers().Where(p => p.PlayerContext.IntroCinematicMode == SetIntroCinematicMode.IntroCinematicMode.WAITING).Take(2).ToArray();

            pairedPlayers[0].PlayerContext.IntroCinematicMode = pairedPlayers[1].PlayerContext.IntroCinematicMode = SetIntroCinematicMode.IntroCinematicMode.START;
            pairedPlayers[0].SendPacket(new SetIntroCinematicMode(pairedPlayers[1].Id, SetIntroCinematicMode.IntroCinematicMode.START));
            pairedPlayers[1].SendPacket(new SetIntroCinematicMode(pairedPlayers[0].Id, SetIntroCinematicMode.IntroCinematicMode.START));
        }
    }
}

using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class SetIntroCinematicModeProcessor : ClientPacketProcessor<SetIntroCinematicMode>
{
    private readonly PlayerManager playerManager;

    public SetIntroCinematicModeProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public override void Process(SetIntroCinematicMode packet)
    {
        if (playerManager.TryFind(packet.PlayerId, out RemotePlayer remotePlayer))
        {
            remotePlayer.PlayerContext.IntroCinematicMode = packet.Mode;
        }
    }
}

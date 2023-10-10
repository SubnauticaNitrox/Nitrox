using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class SetIntroCinematicModeProcessor : ClientPacketProcessor<SetIntroCinematicMode>
{
    private readonly PlayerManager remotePlayerManager;

    public SetIntroCinematicModeProcessor(PlayerManager remotePlayerManager)
    {
        this.remotePlayerManager = remotePlayerManager;
    }

    public override void Process(SetIntroCinematicMode packet)
    {
        if (remotePlayerManager.TryFind(packet.PlayerId, out RemotePlayer remotePlayer))
        {
            remotePlayer.PlayerContext.IntroCinematicMode = packet.Mode;
        }
    }
}

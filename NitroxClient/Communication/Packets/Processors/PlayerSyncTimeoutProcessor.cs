using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerSyncTimeoutProcessor : ClientPacketProcessor<PlayerSyncTimeout>
{
    public override void Process(PlayerSyncTimeout packet)
    {
        Multiplayer.Main.TimeOut();
    }
}

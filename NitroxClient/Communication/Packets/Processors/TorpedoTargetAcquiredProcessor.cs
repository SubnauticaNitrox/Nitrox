using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.Communication.Packets.Processors;

public class TorpedoTargetAcquiredProcessor : ClientPacketProcessor<TorpedoTargetAcquired>
{
    private readonly BulletManager bulletManager;

    public TorpedoTargetAcquiredProcessor(BulletManager bulletManager)
    {
        this.bulletManager = bulletManager;
    }

    public override void Process(TorpedoTargetAcquired packet)
    {
        bulletManager.TorpedoTargetAcquired(packet.BulletId, packet.TargetId, packet.Position.ToUnity(), packet.Rotation.ToUnity());
    }
}

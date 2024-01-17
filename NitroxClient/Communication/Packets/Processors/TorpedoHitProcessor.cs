using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.Communication.Packets.Processors;

public class TorpedoHitProcessor : ClientPacketProcessor<TorpedoHit>
{
    private readonly BulletManager bulletManager;

    public TorpedoHitProcessor(BulletManager bulletManager)
    {
        this.bulletManager = bulletManager;
    }

    public override void Process(TorpedoHit packet)
    {
        bulletManager.TorpedoHit(packet.BulletId, packet.Position.ToUnity(), packet.Rotation.ToUnity());
    }
}

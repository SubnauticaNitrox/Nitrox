using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.Communication.Packets.Processors;

public class StasisSphereHitProcessor : ClientPacketProcessor<StasisSphereHit>
{
    private readonly BulletManager bulletManager;

    public StasisSphereHitProcessor(BulletManager bulletManager)
    {
        this.bulletManager = bulletManager;
    }

    public override void Process(StasisSphereHit packet)
    {
        bulletManager.StasisSphereHit(packet.PlayerId, packet.Position.ToUnity(), packet.Rotation.ToUnity(), packet.ChargeNormalized, packet.Consumption);
    }
}

using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.Communication.Packets.Processors;

public class StasisSphereShotProcessor : ClientPacketProcessor<StasisSphereShot>
{
    private readonly BulletManager bulletManager;

    public StasisSphereShotProcessor(BulletManager bulletManager)
    {
        this.bulletManager = bulletManager;
    }

    public override void Process(StasisSphereShot packet)
    {
        bulletManager.ShootStasisSphere(packet.PlayerId, packet.Position.ToUnity(), packet.Rotation.ToUnity(), packet.Speed, packet.LifeTime, packet.ChargeNormalized);
    }
}

using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;
using Nitrox.Model.Subnautica.Packets;

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

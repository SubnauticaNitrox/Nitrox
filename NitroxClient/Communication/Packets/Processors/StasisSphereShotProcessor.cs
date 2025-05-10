using NitroxClient.GameLogic;
using Nitrox.Model.Subnautica.DataStructures;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class StasisSphereShotProcessor : IClientPacketProcessor<StasisSphereShot>
{
    private readonly BulletManager bulletManager;

    public StasisSphereShotProcessor(BulletManager bulletManager)
    {
        this.bulletManager = bulletManager;
    }

    public Task Process(IPacketProcessContext context, StasisSphereShot packet)
    {
        bulletManager.ShootStasisSphere(packet.PlayerId, packet.Position.ToUnity(), packet.Rotation.ToUnity(), packet.Speed, packet.LifeTime, packet.ChargeNormalized);

        return Task.CompletedTask;
    }
}

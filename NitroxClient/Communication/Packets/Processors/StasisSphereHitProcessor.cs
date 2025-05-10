using NitroxClient.GameLogic;
using Nitrox.Model.Subnautica.DataStructures;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class StasisSphereHitProcessor : IClientPacketProcessor<StasisSphereHit>
{
    private readonly BulletManager bulletManager;

    public StasisSphereHitProcessor(BulletManager bulletManager)
    {
        this.bulletManager = bulletManager;
    }

    public Task Process(IPacketProcessContext context, StasisSphereHit packet)
    {
        bulletManager.StasisSphereHit(packet.PlayerId, packet.Position.ToUnity(), packet.Rotation.ToUnity(), packet.ChargeNormalized, packet.Consumption);

        return Task.CompletedTask;
    }
}

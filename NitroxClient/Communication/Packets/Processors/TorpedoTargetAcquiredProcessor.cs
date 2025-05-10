using NitroxClient.GameLogic;
using Nitrox.Model.Subnautica.DataStructures;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class TorpedoTargetAcquiredProcessor : IClientPacketProcessor<TorpedoTargetAcquired>
{
    private readonly BulletManager bulletManager;

    public TorpedoTargetAcquiredProcessor(BulletManager bulletManager)
    {
        this.bulletManager = bulletManager;
    }

    public Task Process(IPacketProcessContext context, TorpedoTargetAcquired packet)
    {
        bulletManager.TorpedoTargetAcquired(packet.BulletId, packet.TargetId, packet.Position.ToUnity(), packet.Rotation.ToUnity());

        return Task.CompletedTask;
    }
}

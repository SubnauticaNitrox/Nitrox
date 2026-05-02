using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class TorpedoTargetAcquiredProcessor(BulletManager bulletManager) : IClientPacketProcessor<TorpedoTargetAcquired>
{
    private readonly BulletManager bulletManager = bulletManager;

    public Task Process(ClientProcessorContext context, TorpedoTargetAcquired packet)
    {
        bulletManager.TorpedoTargetAcquired(packet.BulletId, packet.TargetId, packet.Position.ToUnity(), packet.Rotation.ToUnity());
        return Task.CompletedTask;
    }
}

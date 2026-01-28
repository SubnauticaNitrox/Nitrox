using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class TorpedoHitProcessor(BulletManager bulletManager) : IClientPacketProcessor<TorpedoHit>
{
    private readonly BulletManager bulletManager = bulletManager;

    public Task Process(ClientProcessorContext context, TorpedoHit packet)
    {
        bulletManager.TorpedoHit(packet.BulletId, packet.Position.ToUnity(), packet.Rotation.ToUnity());
        return Task.CompletedTask;
    }
}

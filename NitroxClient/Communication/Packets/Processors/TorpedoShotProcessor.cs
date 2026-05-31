using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class TorpedoShotProcessor(BulletManager bulletManager) : IClientPacketProcessor<TorpedoShot>
{
    private readonly BulletManager bulletManager = bulletManager;

    public Task Process(ClientProcessorContext context, TorpedoShot packet)
    {
        bulletManager.ShootSeamothTorpedo(packet.BulletId, packet.TechType.ToUnity(), packet.Position.ToUnity(), packet.Rotation.ToUnity(), packet.Speed, packet.LifeTime);
        return Task.CompletedTask;
    }
}

using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class StasisSphereShotProcessor(BulletManager bulletManager) : IClientPacketProcessor<StasisSphereShot>
{
    private readonly BulletManager bulletManager = bulletManager;

    public Task Process(ClientProcessorContext context, StasisSphereShot packet)
    {
        bulletManager.ShootStasisSphere(packet.SessionId, packet.Position.ToUnity(), packet.Rotation.ToUnity(), packet.Speed, packet.LifeTime, packet.ChargeNormalized);
        return Task.CompletedTask;
    }
}

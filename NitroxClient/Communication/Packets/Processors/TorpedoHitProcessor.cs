using NitroxClient.GameLogic;
using Nitrox.Model.Subnautica.DataStructures;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class TorpedoHitProcessor : IClientPacketProcessor<TorpedoHit>
{
    private readonly BulletManager bulletManager;

    public TorpedoHitProcessor(BulletManager bulletManager)
    {
        this.bulletManager = bulletManager;
    }

    public Task Process(IPacketProcessContext context, TorpedoHit packet)
    {
        bulletManager.TorpedoHit(packet.BulletId, packet.Position.ToUnity(), packet.Rotation.ToUnity());

        return Task.CompletedTask;
    }
}

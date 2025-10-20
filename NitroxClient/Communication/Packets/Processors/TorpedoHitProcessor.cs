using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class TorpedoHitProcessor : ClientPacketProcessor<TorpedoHit>
{
    private readonly BulletManager bulletManager;

    public TorpedoHitProcessor(BulletManager bulletManager)
    {
        this.bulletManager = bulletManager;
    }

    public override void Process(TorpedoHit packet)
    {
        bulletManager.TorpedoHit(packet.BulletId, packet.Position.ToUnity(), packet.Rotation.ToUnity());
    }
}

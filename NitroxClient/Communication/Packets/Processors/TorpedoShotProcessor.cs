using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class TorpedoShotProcessor : ClientPacketProcessor<TorpedoShot>
{
    private readonly BulletManager bulletManager;

    public TorpedoShotProcessor(BulletManager bulletManager)
    {
        this.bulletManager = bulletManager;
    }

    public override void Process(TorpedoShot packet)
    {
        bulletManager.ShootSeamothTorpedo(packet.BulletId, packet.TechType.ToUnity(), packet.Position.ToUnity(), packet.Rotation.ToUnity(), packet.Speed, packet.LifeTime);
    }
}

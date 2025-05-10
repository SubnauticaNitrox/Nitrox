using NitroxClient.GameLogic;
using Nitrox.Model.Subnautica.DataStructures;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class TorpedoShotProcessor : IClientPacketProcessor<TorpedoShot>
{
    private readonly BulletManager bulletManager;

    public TorpedoShotProcessor(BulletManager bulletManager)
    {
        this.bulletManager = bulletManager;
    }

    public Task Process(IPacketProcessContext context, TorpedoShot packet)
    {
        bulletManager.ShootSeamothTorpedo(packet.BulletId, packet.TechType.ToUnity(), packet.Position.ToUnity(), packet.Rotation.ToUnity(), packet.Speed, packet.LifeTime);
        return Task.CompletedTask;
    }
}

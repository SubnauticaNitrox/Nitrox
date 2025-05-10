using NitroxClient.MonoBehaviours;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class SeaDragonGrabExosuitProcessor : IClientPacketProcessor<SeaDragonGrabExosuit>
{
    public Task Process(IPacketProcessContext context, SeaDragonGrabExosuit packet)
    {
        if (!NitroxEntity.TryGetComponentFrom(packet.SeaDragonId, out SeaDragon seaDragon) ||
            !NitroxEntity.TryGetComponentFrom(packet.TargetId, out Exosuit exosuit))
        {
            return Task.CompletedTask;
        }

        using (PacketSuppressor<SeaDragonGrabExosuit>.Suppress())
        {
            seaDragon.GrabExosuit(exosuit);
            seaDragon.CancelInvoke(nameof(SeaDragon.DamageExosuit));
        }

        return Task.CompletedTask;
    }
}

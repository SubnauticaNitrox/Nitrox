using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class SeaDragonGrabExosuitProcessor : ClientPacketProcessor<SeaDragonGrabExosuit>
{
    public override void Process(SeaDragonGrabExosuit packet)
    {
        if (!NitroxEntity.TryGetComponentFrom(packet.SeaDragonId, out SeaDragon seaDragon) ||
            !NitroxEntity.TryGetComponentFrom(packet.TargetId, out Exosuit exosuit))
        {
            return;
        }

        using (PacketSuppressor<SeaDragonGrabExosuit>.Suppress())
        {
            seaDragon.GrabExosuit(exosuit);
            seaDragon.CancelInvoke(nameof(SeaDragon.DamageExosuit));
        }
    }
}

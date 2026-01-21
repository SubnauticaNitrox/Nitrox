using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class SeaDragonGrabExosuitProcessor : IClientPacketProcessor<SeaDragonGrabExosuit>
{
    public Task Process(ClientProcessorContext context, SeaDragonGrabExosuit packet)
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

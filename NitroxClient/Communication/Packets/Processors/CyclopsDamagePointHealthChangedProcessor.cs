using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class CyclopsDamagePointHealthChangedProcessor : IClientPacketProcessor<CyclopsDamagePointRepaired>
{
    public Task Process(ClientProcessorContext context, CyclopsDamagePointRepaired packet)
    {
        GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.Id);
        SubRoot cyclops = gameObject.RequireComponent<SubRoot>();

        using (PacketSuppressor<CyclopsDamage>.Suppress())
        using (PacketSuppressor<CyclopsDamagePointRepaired>.Suppress())
        {
            cyclops.damageManager.damagePoints[packet.DamagePointIndex].liveMixin.AddHealth(packet.RepairAmount);
        }
        return Task.CompletedTask;
    }
}

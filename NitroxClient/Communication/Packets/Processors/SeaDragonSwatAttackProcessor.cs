using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class SeaDragonSwatAttackProcessor : IClientPacketProcessor<SeaDragonSwatAttack>
{
    public Task Process(ClientProcessorContext context, SeaDragonSwatAttack packet)
    {
        if (!NitroxEntity.TryGetComponentFrom(packet.SeaDragonId, out SeaDragonMeleeAttack seaDragonMeleeAttack) ||
            !NitroxEntity.TryGetObjectFrom(packet.TargetId, out GameObject targetObject))
        {
            return Task.CompletedTask;
        }

        using (PacketSuppressor<SeaDragonSwatAttack>.Suppress())
        {
            seaDragonMeleeAttack.seaDragon.Aggression.Value = packet.Aggression;
            seaDragonMeleeAttack.SwatAttack(targetObject, packet.IsRightHand);
        }
        return Task.CompletedTask;
    }
}

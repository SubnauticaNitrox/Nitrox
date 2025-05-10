using NitroxClient.MonoBehaviours;
using NitroxModel.Networking.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class SeaDragonSwatAttackProcessor : IClientPacketProcessor<SeaDragonSwatAttack>
{
    public Task Process(IPacketProcessContext context, SeaDragonSwatAttack packet)
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

using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class SeaDragonSwatAttackProcessor : ClientPacketProcessor<SeaDragonSwatAttack>
{
    public override void Process(SeaDragonSwatAttack packet)
    {
        if (!NitroxEntity.TryGetComponentFrom(packet.SeaDragonId, out SeaDragonMeleeAttack seaDragonMeleeAttack) ||
            !NitroxEntity.TryGetObjectFrom(packet.TargetId, out GameObject targetObject))
        {
            return;
        }

        using (PacketSuppressor<SeaDragonSwatAttack>.Suppress())
        {
            seaDragonMeleeAttack.seaDragon.Aggression.Value = packet.Aggression;
            seaDragonMeleeAttack.SwatAttack(targetObject, packet.IsRightHand);
        }
    }
}

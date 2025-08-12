using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class SeaDragonAttackTargetProcessor : ClientPacketProcessor<SeaDragonAttackTarget>
{
    public override void Process(SeaDragonAttackTarget packet)
    {
        if (!NitroxEntity.TryGetComponentFrom(packet.SeaDragonId, out SeaDragonMeleeAttack seaDragonMeleeAttack) ||
            !NitroxEntity.TryGetObjectFrom(packet.TargetId, out GameObject target))
        {
            return;
        }

        seaDragonMeleeAttack.seaDragon.Aggression.Value = packet.Aggression;
        if (target.GetComponent<SubControl>())
        {
            // SeaDragonMeleeAttack.OnTouchFront's useful part about Cyclops attack
            seaDragonMeleeAttack.animator.SetTrigger("shove");
            seaDragonMeleeAttack.SendMessage("OnMeleeAttack", target, SendMessageOptions.DontRequireReceiver);
            seaDragonMeleeAttack.timeLastBite = Time.time;
            return;
        }


        // SeaDragonMeleeAttack.OnTouchFront's useful part about local player attack
        Collider collider;
        if (target.TryGetComponent(out RemotePlayerIdentifier remotePlayerIdentifier))
        {
            collider = remotePlayerIdentifier.RemotePlayer.Collider;
        }
        else if (target.GetComponent<Player>())
        {
            collider = Player.mainCollider;
        }
        else
        {
            return;
        }

        seaDragonMeleeAttack.timeLastBite = Time.time;
        if (seaDragonMeleeAttack.attackSound)
        {
            using (PacketSuppressor<FMODAssetPacket>.Suppress())
            {
                Utils.PlayEnvSound(seaDragonMeleeAttack.attackSound, collider.transform.position, 20f);
            }
        }
        seaDragonMeleeAttack.OnTouch(collider);
    }
}

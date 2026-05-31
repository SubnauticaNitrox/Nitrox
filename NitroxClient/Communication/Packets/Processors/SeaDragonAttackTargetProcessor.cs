using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class SeaDragonAttackTargetProcessor : IClientPacketProcessor<SeaDragonAttackTarget>
{
    public Task Process(ClientProcessorContext context, SeaDragonAttackTarget packet)
    {
        if (!NitroxEntity.TryGetComponentFrom(packet.SeaDragonId, out SeaDragonMeleeAttack seaDragonMeleeAttack) ||
            !NitroxEntity.TryGetObjectFrom(packet.TargetId, out GameObject target))
        {
            return Task.CompletedTask;
        }

        seaDragonMeleeAttack.seaDragon.Aggression.Value = packet.Aggression;
        if (target.GetComponent<SubControl>())
        {
            // SeaDragonMeleeAttack.OnTouchFront's useful part about Cyclops attack
            seaDragonMeleeAttack.animator.SetTrigger("shove");
            seaDragonMeleeAttack.SendMessage("OnMeleeAttack", target, SendMessageOptions.DontRequireReceiver);
            seaDragonMeleeAttack.timeLastBite = Time.time;
            return Task.CompletedTask;
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
            return Task.CompletedTask;
        }

        seaDragonMeleeAttack.timeLastBite = Time.time;
        if (seaDragonMeleeAttack.attackSound)
        {
            using (PacketSuppressor<FMODAssetPacket>.Suppress())
            {
                Utils.PlayEnvSound(seaDragonMeleeAttack.attackSound, collider.transform.position);
            }
        }
        seaDragonMeleeAttack.OnTouch(collider);
        return Task.CompletedTask;
    }
}

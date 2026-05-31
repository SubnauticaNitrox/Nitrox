using System;
using Nitrox.Model.Core;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PvPAttack : Packet
{
    public SessionId TargetSessionId { get; }
    public float Damage { get; set; }
    public AttackType Type { get; }

    public PvPAttack(SessionId targetSessionId, float damage, AttackType type)
    {
        TargetSessionId = targetSessionId;
        Damage = damage;
        Type = type;
    }

    public enum AttackType : byte
    {
        KnifeHit,
        HeatbladeHit
    }
}

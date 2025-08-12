using System;

namespace NitroxModel.Packets;

[Serializable]
public class PvPAttack : Packet
{
    public ushort TargetPlayerId { get; }
    public float Damage { get; set; }
    public AttackType Type { get; }

    public PvPAttack(ushort targetPlayerId, float damage, AttackType type)
    {
        TargetPlayerId = targetPlayerId;
        Damage = damage;
        Type = type;
    }

    public enum AttackType : byte
    {
        KnifeHit,
        HeatbladeHit
    }
}

using System;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record PvpAttack : Packet
{
    public SessionId TargetSessionId { get; }
    public float Damage { get; set; }
    public AttackType Type { get; }

    public PvpAttack(SessionId targetSessionId, float damage, AttackType type)
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

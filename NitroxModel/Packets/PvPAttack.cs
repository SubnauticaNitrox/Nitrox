using System;

namespace NitroxModel.Packets;

[Serializable]
public class PvPAttack : Packet
{
    public string TargetPlayerName { get; }
    public float Damage { get; set; }
    public AttackType Type { get; }

    public PvPAttack(string targetPlayerName, float damage, AttackType type)
    {
        TargetPlayerName = targetPlayerName;
        Damage = damage;
        Type = type;
    }

    public enum AttackType
    {
        KnifeHit,
        HeatbladeHit
    }
}

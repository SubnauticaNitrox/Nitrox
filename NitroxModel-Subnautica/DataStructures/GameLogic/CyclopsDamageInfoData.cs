using System;
using NitroxModel.DataStructures;
using NitroxModel.Serialization;
using UnityEngine;

namespace NitroxModel_Subnautica.DataStructures.GameLogic;

[Serializable]
[JsonContractTransition]
public class CyclopsDamageInfoData
{
    [JsonMemberTransition]
    public NitroxId ReceiverId { get; set; }

    [JsonMemberTransition]
    public NitroxId DealerId { get; set; }

    [JsonMemberTransition]
    public float OriginalDamage { get; set; }

    [JsonMemberTransition]
    public float Damage { get; set; }

    [JsonMemberTransition]
    public Vector3 Position { get; set; }

    [JsonMemberTransition]
    public DamageType Type { get; set; }

    public CyclopsDamageInfoData(NitroxId receiverId, NitroxId dealerId, float originalDamage, float damage, Vector3 position, DamageType type)
    {
        ReceiverId = receiverId;
        DealerId = dealerId;
        OriginalDamage = originalDamage;
        Damage = damage;
        Position = position;
        Type = type;
    }

    public override string ToString()
    {
        return $"[CyclopsDamageInfoData - ReceiverId: {ReceiverId}, DealerId:{DealerId}, OriginalDamage: {OriginalDamage}, Damage: {Damage}, Position: {Position}, Type: {Type}]";
    }
}

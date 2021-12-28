using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using ProtoBufNet;
using UnityEngine;
using ZeroFormatter;

namespace NitroxModel_Subnautica.DataStructures.GameLogic
{
    [ZeroFormattable]
    [ProtoContract]
    public class CyclopsDamageInfoData
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual NitroxId ReceiverId { get; set; }

        [Index(1)]
        [ProtoMember(2)]
        public virtual NitroxId DealerId { get; set; }

        [Index(2)]
        [ProtoMember(3)]
        public virtual float OriginalDamage { get; set; }

        [Index(3)]
        [ProtoMember(4)]
        public virtual float Damage { get; set; }

        [Index(4)]
        [ProtoMember(5)]
        public virtual NitroxVector3 Position { get; set; }

        [Index(5)]
        [ProtoMember(6)]
        public virtual DamageType Type { get; set; }

        public CyclopsDamageInfoData()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public CyclopsDamageInfoData(NitroxId receiverId, NitroxId dealerId, float originalDamage, float damage, NitroxVector3 position, DamageType type)
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
            return $"[CyclopsDamageInfoData - ReceiverId: {ReceiverId} DealerId:{DealerId} OriginalDamage: {OriginalDamage} Damage: {Damage} Position: {Position} Type: {Type}}}]";
        }
    }
}

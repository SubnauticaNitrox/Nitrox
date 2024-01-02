using System;
using System.Runtime.Serialization;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;

namespace NitroxModel_Subnautica.DataStructures.GameLogic
{
    [Serializable]
    [DataContract]
    public class CyclopsDamageInfoData
    {
        [DataMember(Order = 1)]
        public NitroxId ReceiverId { get; set; }

        [DataMember(Order = 2)]
        public Optional<NitroxId> DealerId { get; set; }

        [DataMember(Order = 3)]
        public float OriginalDamage { get; set; }

        [DataMember(Order = 4)]
        public float Damage { get; set; }

        [DataMember(Order = 5)]
        public NitroxVector3 Position { get; set; }

        [DataMember(Order = 6)]
        public DamageType Type { get; set; }

        protected CyclopsDamageInfoData()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public CyclopsDamageInfoData(NitroxId receiverId, Optional<NitroxId> dealerId, float originalDamage, float damage, NitroxVector3 position, DamageType type)
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

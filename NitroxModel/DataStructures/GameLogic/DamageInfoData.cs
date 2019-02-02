﻿using System;
using ProtoBuf;
using UnityEngine;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class DamageInfoData
    {
        [ProtoMember(1)]
        public string ReceiverGuid { get; set; }

        [ProtoMember(2)]
        public string DealerGuid { get; set; }

        [ProtoMember(3)]
        public float OriginalDamage { get; set; }

        [ProtoMember(4)]
        public float Damage { get; set; }

        [ProtoMember(5)]
        public Vector3 Position { get; set; }

        [ProtoMember(6)]
        public DamageType Type { get; set; }

        public DamageInfoData()
        {
            // Default Constructor for serialization
        }

        public DamageInfoData(string receiverGuid, string dealerGuid, float originalDamage, float damage, Vector3 position, DamageType type)
        {
            ReceiverGuid = receiverGuid;
            DealerGuid = dealerGuid;
            OriginalDamage = originalDamage;
            Damage = damage;
            Position = position;
            Type = type;
        }

        public override string ToString()
        {
            return "[DamageInfoData - ReceiverGuid: " + ReceiverGuid 
                + " DealerGuid:" + DealerGuid 
                + " OriginalDamage: " + OriginalDamage 
                + " Damage: " + Damage
                + " Position: " + Position
                + " Type: " + Type
                + "}]";
        }
    }
}

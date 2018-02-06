using System;
using UnityEngine;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    public class SerializableDamageInfo
    {
        public float OriginalDamage;
        public float Damage;
        public Vector3 Position;
        public DamageType Type;
        public string DealerGuid;
    }
}

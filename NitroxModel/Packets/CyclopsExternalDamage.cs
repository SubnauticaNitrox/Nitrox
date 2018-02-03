using System;
using System.Linq;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsExternalDamage : Packet
    {
        public string Guid { get; }
        public int[] DamagePointIndexes { get; }
        public float Health { get; }
        // public bool IsRepair { get; }

        public CyclopsExternalDamage(string guid, float health, int[] damagePointIndexes)//, bool isRepair)
        {
            Guid = guid;
            Health = health;
            DamagePointIndexes = damagePointIndexes;
            // IsRepair = isRepair;
        }

        public override string ToString()
        {
            return "[CyclopsExternalDamage Guid: " + Guid + " Health: " + Health.ToTwoDecimalString() + " DamagePointIndexes: " + string.Join(", ", DamagePointIndexes.Select(x => x.ToString()).ToArray()) + /*" IsRepair: " + IsRepair.ToString() +*/ "]";
        }
    }
}

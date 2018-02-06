using System;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    /// <summary>
    /// If the <see cref="Attacker"/> == null and <see cref="AttackDamage"/> == 0, it's an update caused by a repair, or a callback
    /// from the server to sync where the <see cref="CyclopsDamagePoint"/>(s) should be, neither of which is an attack.
    /// </summary>
    [Serializable]
    public class CyclopsDamage : Packet
    {
        public string Guid { get; }
        public float SubHealth { get; }
        public float DamageManagerHealth { get; }
        public float SubFireHealth { get; }
        public int[] DamagePointIndexes { get; }
        public SerializableRoomFire[] RoomFires { get; }
        public SerializableDamageInfo DamageInfo { get; }

        public CyclopsDamage(string guid, float subHealth, float damageManagerHealth, float subFireHealth, int[] damagePointIndexes, SerializableRoomFire[] roomFires, SerializableDamageInfo damageInfo = null)
        {
            Guid = guid;
            SubHealth = subHealth;
            DamageManagerHealth = damageManagerHealth;
            SubFireHealth = subFireHealth;
            DamagePointIndexes = damagePointIndexes;
            RoomFires = roomFires;
            DamageInfo = damageInfo;
        }

        public override string ToString()
        {
            return "[CyclopsDamage Guid: " + Guid
                + " SubHealth: " + SubHealth.ToTwoDecimalString()
                + " DamageManagerHealth: " + DamageManagerHealth.ToTwoDecimalString()
                + " SubFireHealth: " + SubFireHealth.ToTwoDecimalString()
                + " DamagePointIndexes: " + string.Join(", ", DamagePointIndexes.Select(x => x.ToString()).ToArray())
                + " RoomFires: " + string.Join(", ", RoomFires.Select(x => x.ToString()).ToArray());
            //+ " DamageInfo: DealerGuid: " + DamageInfo.DealerGuid + " Damage: " + DamageInfo.OriginalDamage + "ModifiedDamage:" + DamageInfo.Damage + "Pos:" + DamageInfo.Position.ToString()
        }
    }
}

﻿using System;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    /// <summary>
    /// A state update packet for the Cyclops that could be sent due to a <see cref="CyclopsDamagePoint"/> create/repair, <see cref="SubFire"/> create/extinguish,
    /// or a general Cyclops health change. A health change to 0 means the Cyclops has been destroyed.
    /// </summary>
    [Serializable]
    public class CyclopsDamage : Packet
    {
        public string Guid { get; }
        public float SubHealth { get; }
        public float DamageManagerHealth { get; }
        public float SubFireHealth { get; }
        public int[] DamagePointIndexes { get; }
        public CyclopsFireData[] RoomFires { get; }
        public DamageInfoData DamageInfo { get; }

        /// <param name="guid"><see cref="SubRoot"/> Guid.</param>
        /// <param name="subHealth"><see cref="SubRoot.liveMixin.health"/>.</param>
        /// <param name="damageManagerHealth"><see cref="CyclopsExternalDamageManager.subLiveMixin.health"/>.</param>
        /// <param name="subFireHealth"><see cref="SubFire.liveMixin.health"/>.</param>
        /// <param name="damagePointIndexes"><see cref="CyclopsExternalDamageManager.damagePoints"/> where <see cref="GameObject.activeSelf"/>. 
        ///     Null if only a Cyclops health change.</param>
        /// <param name="roomFires"><see cref="SubFire.RoomFire.spawnNodes"/> where <see cref="Transform.childCount"/> > 0. 
        ///     Null if only a Cyclops health change.</param>
        /// <param name="damageInfo">Null if a repair or extinguish.</param>
        public CyclopsDamage(string guid, float subHealth, float damageManagerHealth, float subFireHealth, int[] damagePointIndexes, CyclopsFireData[] roomFires, DamageInfoData damageInfo = null)
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
                + (DamagePointIndexes == null ? "" : " DamagePointIndexes: " + string.Join(", ", DamagePointIndexes.Select(x => x.ToString()).ToArray()))
                + (RoomFires == null ? "" : " RoomFires: " + string.Join(", ", RoomFires.Select(x => x.ToString()).ToArray()))
                + (DamageInfo == null ? "" : " DamageInfo: DealerGuid: " + DamageInfo?.DealerGuid ?? "null" + " Damage: " + DamageInfo?.OriginalDamage + "ModifiedDamage:" + DamageInfo?.Damage + "Pos:" + DamageInfo?.Position.ToString());
        }
    }
}

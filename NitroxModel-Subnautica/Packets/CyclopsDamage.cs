using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel_Subnautica.Packets
{
    /// <summary>
    /// A state update packet for the Cyclops that could be sent due to a <see cref="CyclopsDamagePoint"/> create/repair, <see cref="SubFire"/> create/extinguish,
    /// or a general Cyclops health change. A health change to 0 means the Cyclops has been destroyed.
    /// </summary>
    [ZeroFormattable]
    public class CyclopsDamage : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual float SubHealth { get; protected set; }
        [Index(2)]
        public virtual float DamageManagerHealth { get; protected set; }
        [Index(3)]
        public virtual float SubFireHealth { get; protected set; }
        [Index(4)]
        public virtual int[] DamagePointIndexes { get; protected set; }
        [Index(5)]
        public virtual CyclopsFireData[] RoomFires { get; protected set; }
        [Index(6)]
        public virtual CyclopsDamageInfoData DamageInfo { get; protected set; }

        public CyclopsDamage() { }

        /// <param name="id"><see cref="SubRoot"/> Id.</param>
        /// <param name="subHealth"><see cref="SubRoot.liveMixin.health"/>.</param>
        /// <param name="damageManagerHealth"><see cref="CyclopsExternalDamageManager.subLiveMixin.health"/>.</param>
        /// <param name="subFireHealth"><see cref="SubFire.liveMixin.health"/>.</param>
        /// <param name="damagePointIndexes"><see cref="CyclopsExternalDamageManager.damagePoints"/> where <see cref="GameObject.activeSelf"/>. 
        ///     Null if only a Cyclops health change.</param>
        /// <param name="roomFires"><see cref="SubFire.RoomFire.spawnNodes"/> where <see cref="Transform.childCount"/> > 0. 
        ///     Null if only a Cyclops health change.</param>
        /// <param name="damageInfo">Null if a repair or extinguish.</param>
        public CyclopsDamage(NitroxId id, float subHealth, float damageManagerHealth, float subFireHealth, int[] damagePointIndexes, CyclopsFireData[] roomFires, CyclopsDamageInfoData damageInfo = null)
        {
            Id = id;
            SubHealth = subHealth;
            DamageManagerHealth = damageManagerHealth;
            SubFireHealth = subFireHealth;
            DamagePointIndexes = damagePointIndexes;
            RoomFires = roomFires;
            DamageInfo = damageInfo;
        }

        public override string ToString()
        {
            return $"[CyclopsDamage - Id: {Id}, SubHealth: {SubHealth}, DamageManagerHealth: {DamageManagerHealth}, SubFireHealth: {SubFireHealth}{(DamagePointIndexes == null ? "" : " DamagePointIndexes: " + string.Join(", ", DamagePointIndexes.Select(x => x.ToString()).ToArray()))}{(RoomFires == null ? "" : " RoomFires: " + string.Join(", ", RoomFires.Select(x => x.ToString()).ToArray()))}{(DamageInfo == null ? "" : " DamageInfo: DealerId: " + DamageInfo?.DealerId ?? "null" + " Damage: " + DamageInfo?.OriginalDamage + "ModifiedDamage:" + DamageInfo?.Damage + "Pos:" + DamageInfo?.Position.ToString())}";
        }
    }
}

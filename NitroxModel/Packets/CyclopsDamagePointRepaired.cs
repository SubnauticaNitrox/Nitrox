using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsDamagePointRepaired : Packet
    {
        public string Guid { get; }
        public int DamagePointIndex { get; }
        public float RepairAmount { get; }

        /// <param name="guid">The Cyclops guid</param>
        /// <param name="repairAmount">The amount to repair the damage by. A large repair amount is passed if the point is meant to be fully repaired</param>
        public CyclopsDamagePointRepaired(string guid, int damagePointIndex, float repairAmount)
        {
            Guid = guid;
            DamagePointIndex = damagePointIndex;
            RepairAmount = repairAmount;
        }
    }
}

using System;

namespace NitroxModel.Packets
{
    /// <summary>
    /// Triggered when a damage point has been repaired. Separate from <see cref="CyclopsFireHealthChanged"/> due to how differently the repair is managed
    /// </summary>
    [Serializable]
    public class CyclopsDamagePointHealthChanged : Packet
    {
        public string Guid { get; }
        public int DamagePointIndex { get; }
        public float RepairAmount { get; }

        /// <param name="guid">The Cyclops guid</param>
        /// <param name="repairAmount">The amount to repair the damage by. A large repair amount is passed if the point is meant to be fully repaired</param>
        public CyclopsDamagePointHealthChanged(string guid, int damagePointIndex, float repairAmount)
        {
            Guid = guid;
            DamagePointIndex = damagePointIndex;
            RepairAmount = repairAmount;
        }
    }
}

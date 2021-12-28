using NitroxModel.DataStructures;
using NitroxModel.Packets;
using ZeroFormatter;

namespace NitroxModel_Subnautica.Packets
{
    [ZeroFormattable]
    public class CyclopsDamagePointRepaired : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual int DamagePointIndex { get; protected set; }
        [Index(2)]
        public virtual float RepairAmount { get; protected set; }

        public CyclopsDamagePointRepaired() { }

        /// <param name="id">The Cyclops id</param>
        /// <param name="repairAmount">The amount to repair the damage by. A large repair amount is passed if the point is meant to be fully repaired</param>
        public CyclopsDamagePointRepaired(NitroxId id, int damagePointIndex, float repairAmount)
        {
            Id = id;
            DamagePointIndex = damagePointIndex;
            RepairAmount = repairAmount;
        }

        public override string ToString()
        {
            return $"[CyclopsDamagePointRepaired - Id: {Id}, DamagePointIndex: {DamagePointIndex}, RepairAmount: {RepairAmount}]";
        }
    }
}

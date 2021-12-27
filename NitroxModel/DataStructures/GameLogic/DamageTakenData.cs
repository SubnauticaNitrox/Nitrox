using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic
{
    [ZeroFormattable]
    public class DamageTakenData
    {
        [Index(0)]
        public virtual NitroxVector3 Position { get; set; }

        [Index(1)]
        public virtual ushort DamageType { get; set; }

        [Index(2)]
        public virtual Optional<NitroxId> DealerId { get; set; }

        public override string ToString()
        {
            return $"DamageTakenData: Position: {Position}, DamageType: {DamageType}, DealerId: {DealerId}";

        }
    }
}

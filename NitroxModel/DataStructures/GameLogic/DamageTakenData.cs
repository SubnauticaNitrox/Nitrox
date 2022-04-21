using System;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    public class DamageTakenData
    {
        public NitroxVector3 Position { get; set; }

        public ushort DamageType { get; set; }

        public Optional<NitroxId> DealerId { get; set; }

        public override string ToString()
        {
            return $"DamageTakenData: Position: {Position}, DamageType: {DamageType}, DealerId: {DealerId}";
        }
    }
}

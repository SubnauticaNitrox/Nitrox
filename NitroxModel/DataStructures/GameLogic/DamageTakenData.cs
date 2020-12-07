using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    public class DamageTakenData
    {
        public NitroxVector3 Position { get; set; }

        public ushort DamageType { get; set; }

        public Optional<NitroxId> DealerId { get; set; }
    }
}

using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic
{
    public class PDAManagerEntry
    {
        public static Dictionary<NitroxTechType, PDAProgressEntry> CachedEntries { get; set; }

        public bool AuroraExplosionTriggered;

        public PDAManagerEntry()
        {
            AuroraExplosionTriggered = false;
        }
    }
}

using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxClient.GameLogic
{
    public class PDACache
    {
        public static Dictionary<NitroxTechType, PDAProgressEntry> CachedEntries { get; set; }
    }
}

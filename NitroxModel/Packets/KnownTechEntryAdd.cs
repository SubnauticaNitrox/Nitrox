using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class KnownTechEntryAdd : Packet
    {
        public enum EntryCategory
        {
            /// <summary>
            /// <b>KnownTech.knownTech</b><br/>
            /// Tech items that you learn about after fulfilling a requirement. Often times, discovering a new KnownTech.analyzedTech item will provide 1 or more KnownTech.knownTech items. (Peeper -> CookedPeeper)
            /// </summary>
            KNOWN = 0,
            /// <summary>
            /// <b>KnownTech.analyzedTech</b><br/>
            /// Tech items that you find in the world and acquire. They often show a notification saying you found/learned about them. (Fish, Resources, etc)
            /// <br/>
            /// </summary>
            ANALYZED = 1
        }

        public NitroxTechType TechType { get; }
        public bool Verbose { get; }
        public EntryCategory Category { get; }
        public List<NitroxTechType> PartialTechTypesToRemove { get; }

        public KnownTechEntryAdd(EntryCategory category, NitroxTechType techType, bool verbose, List<NitroxTechType> partialTechTypesToRemove = null)
        {
            Category = category;
            TechType = techType;
            Verbose = verbose;
            PartialTechTypesToRemove = partialTechTypesToRemove;
        }
    }
}

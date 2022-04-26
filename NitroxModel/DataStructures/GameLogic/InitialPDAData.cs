using System;
using System.Collections.Generic;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    public class InitialPDAData
    {
        public List<NitroxTechType> UnlockedTechTypes { get; set; }
        public List<NitroxTechType> KnownTechTypes { get; set; }

        public List<NitroxTechType> AnalyzedTechTypes { get; set; }
        public List<string> EncyclopediaEntries { get; set; }
        public List<PDAEntry> PartiallyUnlockedTechTypes { get; set; }
        public List<PDALogEntry> PDALogEntries { get; set; }
        public List<PDAProgressEntry> CachedProgress { get; set; }

        protected InitialPDAData()
        {
            //Constructor for serialization. Has to be "protected" for json serialization.
        }

        public InitialPDAData(List<NitroxTechType> unlockedTechTypes, List<NitroxTechType> knownTechTypes, List<NitroxTechType> analyzedTechTypes, List<string> encyclopediaEntries, List<PDAEntry> partiallyUnlockedTechTypes, List<PDALogEntry> pdaLogEntries, List<PDAProgressEntry> cachedProgress)
        {
            UnlockedTechTypes = unlockedTechTypes;
            KnownTechTypes = knownTechTypes;
            AnalyzedTechTypes = analyzedTechTypes;
            EncyclopediaEntries = encyclopediaEntries;
            PartiallyUnlockedTechTypes = partiallyUnlockedTechTypes;
            PDALogEntries = pdaLogEntries;
            CachedProgress = cachedProgress;
        }
    }
}

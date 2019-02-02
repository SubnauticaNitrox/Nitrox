using System;
using System.Collections.Generic;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    public class InitialPdaData
    {
        public List<TechType> UnlockedTechTypes { get; set; }
        public List<TechType> KnownTechTypes { get; set; }
        public List<EncyclopediaEntry> EncyclopediaEntries { get; set; }
        public List<PDAEntry> PartiallyUnlockedTechTypes { get; set; }
        public List<PDALogEntry> PDALogEntries { get; set; }

        public InitialPdaData()
        {
            // Constructor for serialization
        }

        public InitialPdaData(List<TechType> unlockedTechTypes, List<TechType> knownTechTypes, List<EncyclopediaEntry> encyclopediaEntries, List<PDAEntry> partiallyUnlockedTechTypes, List<PDALogEntry> pdaLogEntries)
        {
            UnlockedTechTypes = unlockedTechTypes;
            KnownTechTypes = knownTechTypes;
            EncyclopediaEntries = encyclopediaEntries;
            PartiallyUnlockedTechTypes = partiallyUnlockedTechTypes;
            PDALogEntries = pdaLogEntries;
        }
    }
}

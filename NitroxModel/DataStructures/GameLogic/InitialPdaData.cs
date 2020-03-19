using System;
using System.Collections.Generic;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    public class InitialPDAData
    {
        public List<TechType> UnlockedTechTypes { get; set; }
        public List<TechType> KnownTechTypes { get; set; }
        public List<string> EncyclopediaEntries { get; set; }
        public List<PDAEntry> PartiallyUnlockedTechTypes { get; set; }
        public List<PDALogEntry> PDALogEntries { get; set; }

        public InitialPDAData()
        {
            // Constructor for serialization
        }

        public InitialPDAData(List<TechType> unlockedTechTypes, List<TechType> knownTechTypes, List<string> encyclopediaEntries, List<PDAEntry> partiallyUnlockedTechTypes, List<PDALogEntry> pdaLogEntries)
        {
            UnlockedTechTypes = unlockedTechTypes;
            KnownTechTypes = knownTechTypes;
            EncyclopediaEntries = encyclopediaEntries;
            PartiallyUnlockedTechTypes = partiallyUnlockedTechTypes;
            PDALogEntries = pdaLogEntries;
        }
    }
}

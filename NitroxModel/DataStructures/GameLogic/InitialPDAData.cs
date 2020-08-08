using System;
using System.Collections.Generic;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    public class InitialPDAData
    {
        public List<NitroxTechType> UnlockedTechTypes { get; set; }
        public List<NitroxTechType> KnownTechTypes { get; set; }
        public List<string> EncyclopediaEntries { get; set; }
        public List<PDAEntry> PartiallyUnlockedTechTypes { get; set; }
        public List<PDALogEntry> PDALogEntries { get; set; }

        protected InitialPDAData()
        {
            //Constructor for serialization. Has to be "protected" for json serialization.
        }

        public InitialPDAData(List<NitroxTechType> unlockedTechTypes, List<NitroxTechType> knownTechTypes, List<string> encyclopediaEntries, List<PDAEntry> partiallyUnlockedTechTypes, List<PDALogEntry> pdaLogEntries)
        {
            UnlockedTechTypes = unlockedTechTypes;
            KnownTechTypes = knownTechTypes;
            EncyclopediaEntries = encyclopediaEntries;
            PartiallyUnlockedTechTypes = partiallyUnlockedTechTypes;
            PDALogEntries = pdaLogEntries;
        }

        public override string ToString()
        {
            return $"[InitialPDAData - UnlockedTechTypes: {string.Join(", ", UnlockedTechTypes)} KnownTechTypes: {string.Join(", ", KnownTechTypes)} EncyclopediaEntries: {string.Join(", ", EncyclopediaEntries)} PartiallyUnlockedTechTypes: {string.Join(", ", PartiallyUnlockedTechTypes)} PDALogEntries: {string.Join(", ", PDALogEntries)}]";
        }
    }
}

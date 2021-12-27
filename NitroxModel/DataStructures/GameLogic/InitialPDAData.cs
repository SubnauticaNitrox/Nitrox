using System.Collections.Generic;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic
{
    [ZeroFormattable]
    public class InitialPDAData
    {
        [Index(0)]
        public virtual List<NitroxTechType> UnlockedTechTypes { get; set; }
        [Index(1)]
        public virtual List<NitroxTechType> KnownTechTypes { get; set; }
        [Index(2)]
        public virtual List<string> EncyclopediaEntries { get; set; }
        [Index(3)]
        public virtual List<PDAEntry> PartiallyUnlockedTechTypes { get; set; }
        [Index(4)]
        public virtual List<PDALogEntry> PDALogEntries { get; set; }
        [Index(5)]
        public virtual List<PDAProgressEntry> CachedProgress { get; set; }

        protected InitialPDAData()
        {
            //Constructor for serialization. Has to be "protected" for json serialization.
        }

        public InitialPDAData(List<NitroxTechType> unlockedTechTypes, List<NitroxTechType> knownTechTypes, List<string> encyclopediaEntries, List<PDAEntry> partiallyUnlockedTechTypes, List<PDALogEntry> pdaLogEntries, List<PDAProgressEntry> cachedProgress)
        {
            UnlockedTechTypes = unlockedTechTypes;
            KnownTechTypes = knownTechTypes;
            EncyclopediaEntries = encyclopediaEntries;
            PartiallyUnlockedTechTypes = partiallyUnlockedTechTypes;
            PDALogEntries = pdaLogEntries;
            CachedProgress = cachedProgress;
        }
    }
}

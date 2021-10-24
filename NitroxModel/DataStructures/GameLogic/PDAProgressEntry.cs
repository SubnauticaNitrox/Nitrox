using System;
using System.Collections.Generic;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    /// <summary>
    ///     Entity tech progress. Stores per unique scannable entity.
    /// </summary>
    [Serializable]
    [ProtoContract]
    public class PDAProgressEntry
    {
        [ProtoMember(1)]
        public NitroxTechType TechType { get; set; }

        [ProtoMember(2)]
        public Dictionary<NitroxId, float> Entries { get; set; }

        protected PDAProgressEntry()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public PDAProgressEntry(NitroxTechType techType, Dictionary<NitroxId, float> entries)
        {
            TechType = techType;
            Entries = entries;
        }
    }
}

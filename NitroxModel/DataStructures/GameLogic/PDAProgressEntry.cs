using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic
{
    /// <summary>
    ///     Entity tech progress. Stores per unique scannable entity.
    /// </summary>
    [Serializable]
    [DataContract]
    public class PDAProgressEntry
    {
        [DataMember(Order = 1)]
        public NitroxTechType TechType { get; set; }

        [DataMember(Order = 2)]
        public Dictionary<NitroxId, float> Entries { get; set; }

        [IgnoreConstructor]
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

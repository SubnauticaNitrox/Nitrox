using System.Collections.Generic;
using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic
{
    /// <summary>
    ///     Entity tech progress. Stores per unique scannable entity.
    /// </summary>
    [ZeroFormattable]
    [ProtoContract]
    public class PDAProgressEntry
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual NitroxTechType TechType { get; set; }

        [Index(1)]
        [ProtoMember(2)]
        public virtual Dictionary<NitroxId, float> Entries { get; set; }

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

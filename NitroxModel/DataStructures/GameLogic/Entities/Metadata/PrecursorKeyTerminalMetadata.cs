using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [ProtoContract]
    public class PrecursorKeyTerminalMetadata : EntityMetadata
    {
        [ProtoMember(1)]
        public bool Slotted { get; }

        protected PrecursorKeyTerminalMetadata()
        {
            //Constructor for serialization. Has to be "protected" for json serialization.
        }

        public PrecursorKeyTerminalMetadata(bool slotted)
        {
            Slotted = slotted;
        }

        public override string ToString()
        {
            return "[PrecursorKeyTerminalMetadata Slotted: " + Slotted + "]";
        }
    }
}

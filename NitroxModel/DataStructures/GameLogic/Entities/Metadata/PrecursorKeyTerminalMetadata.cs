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

        public PrecursorKeyTerminalMetadata()
        {
            // Constructor for serialization
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

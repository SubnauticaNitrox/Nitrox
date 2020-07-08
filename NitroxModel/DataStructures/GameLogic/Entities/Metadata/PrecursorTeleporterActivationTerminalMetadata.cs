using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [ProtoContract]
    public class PrecursorTeleporterActivationTerminalMetadata : EntityMetadata
    {
        [ProtoMember(1)]
        public bool Unlocked { get; }

        public PrecursorTeleporterActivationTerminalMetadata()
        {
            // Constructor for serialization
        }

        public PrecursorTeleporterActivationTerminalMetadata(bool unlocked)
        {
            Unlocked = unlocked;
        }

        public override string ToString()
        {
            return "[PrecursorTeleporterActivationTerminalMetadata Unlocked: " + Unlocked + "]";
        }
    }
}

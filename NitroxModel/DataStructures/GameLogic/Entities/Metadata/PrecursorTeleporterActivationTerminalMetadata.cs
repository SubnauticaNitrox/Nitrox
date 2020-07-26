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

        protected PrecursorTeleporterActivationTerminalMetadata()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
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

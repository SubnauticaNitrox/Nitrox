using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [DataContract]
    public class PrecursorTeleporterActivationTerminalMetadata : EntityMetadata
    {
        [DataMember(Order = 1)]
        public bool Unlocked { get; }

        [IgnoreConstructor]
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
            return $"[PrecursorTeleporterActivationTerminalMetadata Unlocked: {Unlocked}]";
        }
    }
}

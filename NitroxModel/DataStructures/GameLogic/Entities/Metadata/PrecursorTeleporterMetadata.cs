using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [DataContract]
    public class PrecursorTeleporterMetadata : EntityMetadata
    {
        [DataMember(Order = 1)]
        public bool IsOpen { get; }

        [IgnoreConstructor]
        protected PrecursorTeleporterMetadata()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public PrecursorTeleporterMetadata(bool isOpen)
        {
            IsOpen = isOpen;
        }

        public override string ToString()
        {
            return $"[PrecursorTeleporterMetadata isOpen: {IsOpen}]";
        }
    }
}

using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [DataContract]
    public class PrecursorDoorwayMetadata : EntityMetadata
    {
        [DataMember(Order = 1)]
        public bool IsOpen { get; }

        [IgnoreConstructor]
        protected PrecursorDoorwayMetadata()
        {
            //Constructor for serialization. Has to be "protected" for json serialization.
        }

        public PrecursorDoorwayMetadata(bool isOpen)
        {
            IsOpen = isOpen;
        }

        public override string ToString()
        {
            return $"[PrecursorDoorwayMetadata isOpen: {IsOpen}]";
        }
    }
}

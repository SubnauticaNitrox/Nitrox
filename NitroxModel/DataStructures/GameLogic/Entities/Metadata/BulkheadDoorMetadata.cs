using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [DataContract]
    public class BulkheadDoorMetadata : EntityMetadata
    {
        [DataMember(Order = 1)]
        public bool IsOpen { get; }

        [IgnoreConstructor]
        protected BulkheadDoorMetadata()
        {
            //Constructor for serialization. Has to be "protected" for json serialization.
        }

        public BulkheadDoorMetadata(bool isOpen)
        {
            IsOpen = isOpen;
        }

        public override string ToString()
        {
            return $"[BulkheadDoorMetadata IsOpen: {IsOpen}]";
        }
    }
}

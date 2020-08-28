using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [ProtoContract]
    public class BulkheadDoorMetadata : EntityMetadata
    {
        [ProtoMember(1)]
        public bool IsOpen { get; }

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
            return $"[BulkheadDoorMetadata - IsOpen: {IsOpen}]";
        }
    }
}

using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [ProtoContract]
    public class SealedDoorMetadata : EntityMetadata
    {
        [ProtoMember(1)]
        public bool Sealed { get; }
        
        public SealedDoorMetadata()
        {
            // Constructor for serialization
        }

        public SealedDoorMetadata(bool Sealed)
        {
            this.Sealed = Sealed;
        }
    }
}

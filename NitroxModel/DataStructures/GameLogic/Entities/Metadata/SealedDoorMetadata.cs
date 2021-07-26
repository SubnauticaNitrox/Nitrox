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

        [ProtoMember(2)]
        public float OpenedAmount { get; }

        protected SealedDoorMetadata()
        {
            //Constructor for serialization. Has to be "protected" for json serialization.
        }

        public SealedDoorMetadata(bool Sealed, float OpenedAmount)
        {
            this.Sealed = Sealed;
            this.OpenedAmount = OpenedAmount;
        }
    }
}

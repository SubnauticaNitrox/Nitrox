using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [DataContract]
    public class SealedDoorMetadata : EntityMetadata
    {
        [DataMember(Order = 1)]
        public bool Sealed { get; }

        [DataMember(Order = 2)]
        public float OpenedAmount { get; }

        [IgnoreConstructor]
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

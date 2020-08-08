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

        public SealedDoorMetadata(bool @sealed, float openedAmount)
        {
            Sealed = @sealed;
            OpenedAmount = openedAmount;
        }

        public override string ToString()
        {
            return $"[SealedDoorMetadata - Sealed: {Sealed} OpenedAmount: {OpenedAmount}]";
        }
    }
}

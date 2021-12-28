using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [ZeroFormattable]
    [ProtoContract]
    public class SealedDoorMetadata : EntityMetadata
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual bool Sealed { get; protected set; }

        [Index(1)]
        [ProtoMember(2)]
        public virtual float OpenedAmount { get; protected set; }

        public SealedDoorMetadata()
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

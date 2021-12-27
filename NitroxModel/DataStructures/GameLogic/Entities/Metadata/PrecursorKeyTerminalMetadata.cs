using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [ZeroFormattable]
    [ProtoContract]
    public class PrecursorKeyTerminalMetadata : EntityMetadata
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual bool Slotted { get; protected set; }

        protected PrecursorKeyTerminalMetadata()
        {
            //Constructor for serialization. Has to be "protected" for json serialization.
        }

        public PrecursorKeyTerminalMetadata(bool slotted)
        {
            Slotted = slotted;
        }

        public override string ToString()
        {
            return "[PrecursorKeyTerminalMetadata Slotted: " + Slotted + "]";
        }
    }
}

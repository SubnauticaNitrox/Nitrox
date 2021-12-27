using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [ZeroFormattable]
    [ProtoContract]
    public class PrecursorTeleporterMetadata : EntityMetadata
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual bool IsOpen { get; protected set; }

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
            return "[PrecursorTeleporterMetadata isOpen: " + IsOpen + "]";
        }
    }
}

using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [ZeroFormattable]
    [ProtoContract]
    public class PrecursorTeleporterActivationTerminalMetadata : EntityMetadata
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual bool Unlocked { get; protected set; }

        protected PrecursorTeleporterActivationTerminalMetadata()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public PrecursorTeleporterActivationTerminalMetadata(bool unlocked)
        {
            Unlocked = unlocked;
        }

        public override string ToString()
        {
            return "[PrecursorTeleporterActivationTerminalMetadata Unlocked: " + Unlocked + "]";
        }
    }
}

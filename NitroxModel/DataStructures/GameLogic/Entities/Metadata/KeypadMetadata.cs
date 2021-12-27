using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [ZeroFormattable]
    [ProtoContract]
    public class KeypadMetadata : EntityMetadata
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual bool Unlocked { get; protected set; }

        protected KeypadMetadata()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public KeypadMetadata(bool unlocked)
        {
            Unlocked = unlocked;
        }

        public override string ToString()
        {
            return "[KeypadMetadata isOpen: " + Unlocked + "]";
        }
    }
}

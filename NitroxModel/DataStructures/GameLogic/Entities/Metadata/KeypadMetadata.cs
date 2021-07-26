using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [ProtoContract]
    public class KeypadMetadata : EntityMetadata
    {
        [ProtoMember(1)]
        public bool Unlocked { get; }

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

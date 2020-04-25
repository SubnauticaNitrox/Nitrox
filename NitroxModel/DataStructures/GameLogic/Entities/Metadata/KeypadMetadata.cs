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
        
        public KeypadMetadata()
        {
            // Constructor for serialization
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

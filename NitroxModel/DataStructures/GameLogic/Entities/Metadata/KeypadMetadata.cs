using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [DataContract]
    public class KeypadMetadata : EntityMetadata
    {
        [DataMember(Order = 1)]
        public bool Unlocked { get; }

        [IgnoreConstructor]
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
            return $"[KeypadMetadata isOpen: {Unlocked}]";
        }
    }
}

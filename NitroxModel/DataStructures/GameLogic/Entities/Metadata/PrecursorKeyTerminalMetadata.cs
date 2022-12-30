using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [DataContract]
    public class PrecursorKeyTerminalMetadata : EntityMetadata
    {
        [DataMember(Order = 1)]
        public bool Slotted { get; }

        [IgnoreConstructor]
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
            return $"[PrecursorKeyTerminalMetadata Slotted: {Slotted}]";
        }
    }
}

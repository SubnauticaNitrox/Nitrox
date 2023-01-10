using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [DataContract]
    public class IncubatorMetadata : EntityMetadata
    {
        [DataMember(Order = 1)]
        public bool Powered { get; }

        [DataMember(Order = 2)]
        public bool Hatched { get; }

        [IgnoreConstructor]
        protected IncubatorMetadata()
        {
            //Constructor for serialization. Has to be "protected" for json serialization.
        }

        public IncubatorMetadata(bool powered, bool hatched)
        {
            Powered = powered;
            Hatched = hatched;
        }

        public override string ToString()
        {
            return $"[IncubatorMetadata Powered: {Powered} Hatched: {Hatched}]";
        }
    }
}

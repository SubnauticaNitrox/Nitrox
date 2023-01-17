using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata
{
    [Serializable]
    [DataContract]
    public class MapRoomBuilderMetadata : BuilderMetadata
    {
        // The map room internally maintains a cellType and connectionMask for rotation - these values
        // are updated when the scroll wheel changes.
        [DataMember(Order = 1)]
        public byte CellType { get; set; }

        [DataMember(Order = 2)]
        public int Rotation { get; set; }

        [IgnoreConstructor]
        protected MapRoomBuilderMetadata()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public MapRoomBuilderMetadata(byte cellType, int rotation)
        {
            CellType = cellType;
            Rotation = rotation;
        }

        public override string ToString()
        {
            return $"[MapRoomRotationMetadata - CellType: {CellType}, Rotation: {Rotation}]";
        }
    }
}

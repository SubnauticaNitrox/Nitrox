using System;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using ProtoBufNet;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata
{
    [Serializable]
    [ProtoContract]
    public class MapRoomBuilderMetadata : BuilderMetadata
    {
        // The map room internally maintains a cellType and connectionMask for rotation - these values
        // are updated when the scroll wheel changes.
        [ProtoMember(1)]
        public byte CellType { get; set; }

        [ProtoMember(2)]
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

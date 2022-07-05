using System;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using ProtoBufNet;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation
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
        public int ConnectionMask { get; set; }

        protected MapRoomBuilderMetadata() : base(typeof(BaseAddMapRoomGhost))
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public MapRoomBuilderMetadata(byte cellType, int connectionMask) : base(typeof(BaseAddMapRoomGhost))
        {
            CellType = cellType;
            ConnectionMask = connectionMask;
        }

        public override string ToString()
        {
            return $"[MapRoomRotationMetadata - CellType: {CellType}, ConnectionMask: {ConnectionMask}]";
        }
    }
}

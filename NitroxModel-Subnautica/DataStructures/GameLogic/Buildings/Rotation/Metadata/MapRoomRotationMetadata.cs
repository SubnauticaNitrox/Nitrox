using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation
{
    [ZeroFormattable]
    [ProtoContract]
    public class MapRoomRotationMetadata : RotationMetadata
    {
        // The map room internally maintains a cellType and connectionMask for rotation - these values
        // are updated when the scroll wheel changes.
        [Index(0)]
        [ProtoMember(1)]
        public virtual byte CellType { get; set; }

        [Index(1)]
        [ProtoMember(2)]
        public virtual int ConnectionMask { get; set; }

        public MapRoomRotationMetadata() : base(typeof(BaseAddMapRoomGhost))
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public MapRoomRotationMetadata(byte cellType, int connectionMask) : base(typeof(BaseAddMapRoomGhost))
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

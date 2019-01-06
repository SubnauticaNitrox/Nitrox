using ProtoBuf;
using System;

namespace NitroxModel.DataStructures.GameLogic.Buildings.Rotation
{
    [Serializable]
    [ProtoContract]
    public class MapRoomRotationMetadata : RotationMetadata
    {
        // The map room internally maintains a cellType and connectionMask for rotation - these values
        // are updated when the scroll wheel changes.
        [ProtoMember(1)]
        public Base.CellType CellType { get; set; }

        [ProtoMember(2)]
        public int ConnectionMask { get; set; }

        public MapRoomRotationMetadata() : base(typeof(BaseAddMapRoomGhost))
        {
            // For serialization purposes
        }

        public MapRoomRotationMetadata(Base.CellType cellType, int connectionMask) : base (typeof(BaseAddMapRoomGhost))
        {
            CellType = cellType;
            ConnectionMask = connectionMask;
        }

        public override string ToString()
        {
            return "[MapRoomRotationMetadata CellType: " + CellType + " ConnectionMask: " + ConnectionMask + " ]";
        }
    }
}

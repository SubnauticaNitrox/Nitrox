using ProtoBufNet;
using System;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation
{
    [Serializable]
    [ProtoContract]
    public class BaseMapRoomRotationMetadata : RotationMetadata
    {
        public const long VERSION = 2;

        // The map room internally maintains a cellType and connectionMask for rotation - these values
        // are updated when the scroll wheel changes.
        [ProtoMember(1)]
        public byte CellType { get; set; }

        [ProtoMember(2)]
        public int ConnectionMask { get; set; }

        public BaseMapRoomRotationMetadata()
        {
            // For serialization purposes
        }

        public BaseMapRoomRotationMetadata(byte cellType, int connectionMask)
        {
            CellType = cellType;
            ConnectionMask = connectionMask;
        }

        public override string ToString()
        {
            return "[BaseMapRoomRotationMetadata CellType: " + CellType + " ConnectionMask: " + ConnectionMask + " ]";
        }
    }
}

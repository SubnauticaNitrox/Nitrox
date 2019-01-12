using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using ProtoBuf;
using System;

namespace NitroxModel.DataStructures.GameLogic.Buildings.Rotation
{
    [Serializable]
    [ProtoContract, ProtoInclude(50, typeof(CorridorRotationMetadata)), ProtoInclude(60, typeof(MapRoomRotationMetadata))]
    public abstract class RotationMetadata
    {
        [ProtoIgnore]
        public Type GhostType { get; set; }

        public RotationMetadata(Type ghostType)
        {
            GhostType = ghostType;
        }

        public static Optional<RotationMetadata> From(BaseGhost baseGhost)
        {
            RotationMetadata rotationMetadata = null;

            if (baseGhost is BaseAddCorridorGhost)
            {
                BaseAddCorridorGhost corridorGhost = baseGhost as BaseAddCorridorGhost;
                int rotation = (int)corridorGhost.ReflectionGet("rotation");
                rotationMetadata = new CorridorRotationMetadata(rotation);
            }
            else if(baseGhost is BaseAddMapRoomGhost)
            {
                BaseAddMapRoomGhost mapRoomGhost = baseGhost as BaseAddMapRoomGhost;
                Base.CellType cellType = (Base.CellType)mapRoomGhost.ReflectionGet("cellType");
                int connectionMask = (int)mapRoomGhost.ReflectionGet("connectionMask");
                rotationMetadata = new MapRoomRotationMetadata(cellType, connectionMask);
            }

            return Optional<RotationMetadata>.OfNullable(rotationMetadata);
        }
    }
}

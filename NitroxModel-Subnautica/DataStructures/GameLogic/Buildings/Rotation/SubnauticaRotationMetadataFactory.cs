using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation
{
    public class SubnauticaRotationMetadataFactory : RotationMetadataFactory
    {
        public Optional<RotationMetadata> From(object baseGhost)
        {
            RotationMetadata rotationMetadata = null;

            if (baseGhost is BaseAddCorridorGhost)
            {
                BaseAddCorridorGhost corridorGhost = baseGhost as BaseAddCorridorGhost;
                int rotation = (int)corridorGhost.ReflectionGet("rotation");
                rotationMetadata = new CorridorRotationMetadata(rotation);
            }
            else if (baseGhost is BaseAddMapRoomGhost)
            {
                BaseAddMapRoomGhost mapRoomGhost = baseGhost as BaseAddMapRoomGhost;
                Base.CellType cellType = (Base.CellType)mapRoomGhost.ReflectionGet("cellType");
                int connectionMask = (int)mapRoomGhost.ReflectionGet("connectionMask");
                rotationMetadata = new MapRoomRotationMetadata((byte)cellType, connectionMask);
            }

            return Optional.OfNullable(rotationMetadata);
        }
    }
}

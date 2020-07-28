using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation
{
    public class SubnauticaRotationMetadataFactory : RotationMetadataFactory
    {
        public Optional<RotationMetadata> From(object o)
        {
            RotationMetadata rotationMetadata = null;

            if (o is BaseAddCorridorGhost)
            {
                BaseAddCorridorGhost corridorGhost = o as BaseAddCorridorGhost;
                int rotation = (int)corridorGhost.ReflectionGet("rotation");
                rotationMetadata = new CorridorRotationMetadata(rotation);
            }
            else if (o is BaseAddMapRoomGhost)
            {
                BaseAddMapRoomGhost mapRoomGhost = o as BaseAddMapRoomGhost;
                Base.CellType cellType = (Base.CellType)mapRoomGhost.ReflectionGet("cellType");
                int connectionMask = (int)mapRoomGhost.ReflectionGet("connectionMask");
                rotationMetadata = new MapRoomRotationMetadata((byte)cellType, connectionMask);
            }
            else if (o is BaseAddModuleGhost)
            {
                BaseAddModuleGhost module = o as BaseAddModuleGhost;

                Int3 cell = module.anchoredFace.Value.cell;
                int direction = (int)module.anchoredFace.Value.direction;

                rotationMetadata = new BaseModuleRotationMetadata(cell, direction);
            }

            return Optional.OfNullable(rotationMetadata);
        }
    }
}

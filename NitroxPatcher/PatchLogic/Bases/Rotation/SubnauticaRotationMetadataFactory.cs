using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation;

namespace NitroxPatcher.PatchLogic.Bases.Rotation
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
                rotationMetadata = new BaseCorridorRotationMetadata(rotation);
            }
            else if (baseGhost is BaseAddMapRoomGhost)
            {
                BaseAddMapRoomGhost mapRoomGhost = baseGhost as BaseAddMapRoomGhost;
                Base.CellType cellType = (Base.CellType)mapRoomGhost.ReflectionGet("cellType");
                int connectionMask = (int)mapRoomGhost.ReflectionGet("connectionMask");
                rotationMetadata = new BaseMapRoomRotationMetadata((byte)cellType, connectionMask);
            }
            else if (baseGhost is BaseAddModuleGhost)
            {
                BaseAddModuleGhost moduleGhost = baseGhost as BaseAddModuleGhost;

                Int3 cell = moduleGhost.anchoredFace.Value.cell;
                int direction = (int)moduleGhost.anchoredFace.Value.direction;
                int faceType = (int)moduleGhost.faceType;
                int moduleDirection = (int)moduleGhost.ReflectionGet("direction", false);

                rotationMetadata = new BaseModuleRotationMetadata(cell, direction, faceType, moduleDirection);
            }
            else if (baseGhost is BaseAddFaceGhost)
            {
                BaseAddFaceGhost faceGhost = baseGhost as BaseAddFaceGhost;

                Int3 cell = faceGhost.anchoredFace.Value.cell;
                int direction = (int)faceGhost.anchoredFace.Value.direction;
                int faceType = (int)faceGhost.faceType;

                rotationMetadata = new BaseFaceRotationMetadata(cell, direction, faceType);
            }

            return Optional.OfNullable(rotationMetadata);
        }
    }
}

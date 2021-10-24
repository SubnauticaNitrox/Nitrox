﻿using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata;

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
                int rotation = (int)corridorGhost.rotation;
                rotationMetadata = new CorridorRotationMetadata(rotation);
            }
            else if (baseGhost is BaseAddMapRoomGhost)
            {
                BaseAddMapRoomGhost mapRoomGhost = baseGhost as BaseAddMapRoomGhost;
                Base.CellType cellType = mapRoomGhost.cellType;
                int connectionMask = mapRoomGhost.connectionMask;
                rotationMetadata = new MapRoomRotationMetadata((byte)cellType, connectionMask);
            }
            else if (baseGhost is BaseAddModuleGhost)
            {
                BaseAddModuleGhost module = baseGhost as BaseAddModuleGhost;

                Int3 cell = module.anchoredFace.Value.cell;
                int direction = (int)module.anchoredFace.Value.direction;

                rotationMetadata = new BaseModuleRotationMetadata(cell.ToDto(), direction);
            }
            else if (baseGhost is BaseAddFaceGhost)
            {
                BaseAddFaceGhost faceGhost = baseGhost as BaseAddFaceGhost;

                if (faceGhost.anchoredFace.HasValue)
                {
                    Base.Face anchoredFace = faceGhost.anchoredFace.Value;

                    rotationMetadata = new AnchoredFaceRotationMetadata(anchoredFace.cell.ToDto(), (int)anchoredFace.direction, (int)faceGhost.faceType);
                }

            }

            return Optional.OfNullable(rotationMetadata);
        }
    }
}

using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata;
using UnityEngine;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation
{
    public class SubnauticaRotationMetadataFactory : RotationMetadataFactory
    {
        public Optional<BuilderMetadata> From(object baseGhost)
        {
            BuilderMetadata builderMetadata = null;

            switch (baseGhost)
            {
                case BaseAddCorridorGhost corridorGhost:
                {
                    int rotation = corridorGhost.rotation;
                    Vector3 position = corridorGhost.GetComponentInParent<ConstructableBase>().transform.position;
                    bool hasTargetBase = corridorGhost.targetBase != null;
                    Int3 targetCell = hasTargetBase ? corridorGhost.targetBase.WorldToGrid(position): default;
                    builderMetadata = new CorridorBuilderMetadata(position.ToDto(), rotation, hasTargetBase, targetCell.ToDto());
                    break;
                }
                case BaseAddMapRoomGhost mapRoomGhost:
                {
                    Base.CellType cellType = mapRoomGhost.cellType;
                    int connectionMask = mapRoomGhost.connectionMask;
                    builderMetadata = new MapRoomBuilderMetadata((byte)cellType, connectionMask);
                    break;
                }
                case BaseAddModuleGhost module:
                {
                    Int3 cell = module.anchoredFace!.Value.cell;
                    int direction = (int)module.anchoredFace.Value.direction;

                    builderMetadata = new BaseModuleBuilderMetadata(cell.ToDto(), direction);
                    break;
                }
                case BaseAddFaceGhost faceGhost:
                {
                    Base.Face anchoredFace = faceGhost.anchoredFace!.Value;
                    builderMetadata = new AnchoredFaceBuilderMetadata(anchoredFace.cell.ToDto(), (int)anchoredFace.direction, (int)faceGhost.faceType);
                    break;
                }
            }

            return Optional.OfNullable(builderMetadata);
        }
    }
}

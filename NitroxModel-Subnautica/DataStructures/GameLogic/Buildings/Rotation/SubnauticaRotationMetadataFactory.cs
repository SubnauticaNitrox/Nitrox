using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Util;
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
                    Vector3 position = corridorGhost.GetComponentInParent<ConstructableBase>().transform.position;
                    bool hasTargetBase = corridorGhost.targetBase != null;
                    Int3 targetCell = hasTargetBase ? corridorGhost.targetBase.WorldToGrid(position): default;
                    builderMetadata = new CorridorBuilderMetadata(position.ToDto(), Builder.lastRotation, hasTargetBase, targetCell.ToDto());
                    break;
                }
                case BaseAddMapRoomGhost mapRoomGhost:
                {
                    Base.CellType cellType = mapRoomGhost.GetCellType();
                    builderMetadata = new MapRoomBuilderMetadata((byte)cellType, Builder.lastRotation);
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
                    builderMetadata = new AnchoredFaceBuilderMetadata(anchoredFace.cell.ToDto(), (int)anchoredFace.direction, (int)faceGhost.faceType, faceGhost.targetBase.GetAnchor().ToDto());
                    break;
                }
            }

            return Optional.OfNullable(builderMetadata);
        }
    }
}

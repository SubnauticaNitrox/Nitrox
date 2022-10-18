using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Bases.Spawning.BasePiece;
using NitroxClient.GameLogic.Bases.Spawning.Furniture;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxClient.GameLogic
{
    public class Building
    {
        private readonly IPacketSender packetSender;
        private readonly RotationMetadataFactory rotationMetadataFactory;

        public Building(IPacketSender packetSender, RotationMetadataFactory rotationMetadataFactory)
        {
            this.packetSender = packetSender;
            this.rotationMetadataFactory = rotationMetadataFactory;
        }

        public void PlaceBasePiece(BaseGhost baseGhost, ConstructableBase constructableBase, Base targetBase, TechType techType, Quaternion quaternion)
        {
            if (!Builder.isPlacing) //prevent possible echoing
            {
                return;
            }

            NitroxId id = NitroxEntity.GetId(constructableBase.gameObject);

            NitroxId parentBaseId = null;

            if (baseGhost != null)
            {
                if (baseGhost.TargetBase != null)
                {
                    parentBaseId = NitroxEntity.GetId(baseGhost.TargetBase.gameObject);
                }
                else if (baseGhost.GhostBase != null)
                {
                    parentBaseId = NitroxEntity.GetId(baseGhost.GhostBase.gameObject);
                }
            }

            if (parentBaseId == null)
            {
                Base aBase = constructableBase.gameObject.GetComponentInParent<Base>();
                if (aBase != null)
                {
                    parentBaseId = NitroxEntity.GetId(aBase.gameObject);
                }
            }

            Vector3 placedPosition = constructableBase.gameObject.transform.position;
            Transform camera = Camera.main!.transform;
            Optional<BuilderMetadata> rotationMetadata = rotationMetadataFactory.From(baseGhost);

            BasePiece basePiece = new BasePiece(id, placedPosition.ToDto(), quaternion.ToDto(), camera.position.ToDto(), camera.rotation.ToDto(), techType.ToDto(), Optional.OfNullable(parentBaseId), false, rotationMetadata);
            PlaceBasePiece placedBasePiece = new PlaceBasePiece(basePiece);
            packetSender.Send(placedBasePiece);
        }

        public void PlaceFurniture(GameObject gameObject, TechType techType, Vector3 itemPosition, Quaternion quaternion)
        {
            if (!Builder.isPlacing) //prevent possible echoing
            {
                return;
            }

            NitroxId id = NitroxEntity.GetId(gameObject);
            NitroxId parentId = null;

            SubRoot sub = Player.main.currentSub;

            if (sub != null)
            {
                parentId = NitroxEntity.GetId(sub.gameObject);
            }
            else
            {
                Base playerBase = gameObject.GetComponentInParent<Base>();

                if (playerBase != null)
                {
                    parentId = NitroxEntity.GetId(playerBase.gameObject);
                }
            }

            // Leverage local position when in a cyclops as items must be relative.
            bool inCyclops = (sub != null && sub.isCyclops);
            Vector3 position = (inCyclops) ? gameObject.transform.localPosition : itemPosition;
            Quaternion rotation = (inCyclops) ? gameObject.transform.localRotation : quaternion;

            Transform camera = Camera.main!.transform;
            BasePiece basePiece = new BasePiece(id, position.ToDto(), rotation.ToDto(), camera.position.ToDto(), camera.rotation.ToDto(), techType.ToDto(), Optional.OfNullable(parentId), true, Optional.Empty);
            PlaceBasePiece placedBasePiece = new PlaceBasePiece(basePiece);
            packetSender.Send(placedBasePiece);
        }

        public void ChangeConstructionAmount(GameObject gameObject, float amount)
        {
            if (amount is >= 0.99f or <= 0.01f)
            {
                return;
            }

            NitroxId id = NitroxEntity.GetId(gameObject);
            ConstructionAmountChanged amountChanged = new ConstructionAmountChanged(id, amount);
            packetSender.Send(amountChanged);
        }

        public void ConstructionComplete(GameObject ghost, Optional<Base> lastTargetBase, Int3 lastTargetBaseOffset, Base.Face lastTargetFace = default(Base.Face))
        {
            NitroxId baseId = null;
            Optional<object> opConstructedBase = TransientLocalObjectManager.Get(TransientObjectType.BASE_GHOST_NEWLY_CONSTRUCTED_BASE_GAMEOBJECT);

            NitroxId id = NitroxEntity.GetId(ghost);

            if (opConstructedBase.HasValue)
            {
                GameObject constructedBase = (GameObject)opConstructedBase.Value;
                baseId = NitroxEntity.GetId(constructedBase);
            }

            // For base pieces, we must switch the id from the ghost to the newly constructed piece.
            // Furniture just uses the same game object as the ghost for the final product.
            if (ghost.TryGetComponent(out ConstructableBase constructableBase))
            {
                Int3 latestCell = lastTargetBaseOffset;
                Base latestBase = lastTargetBase.HasValue ? lastTargetBase.Value : ((GameObject)opConstructedBase.Value).GetComponent<Base>();
                baseId = NitroxEntity.GetId(latestBase.gameObject);

                Transform cellTransform;
                GameObject placedPiece = null;
                
                if (!latestBase)
                {
                    if (opConstructedBase.HasValue)
                    {
                        latestBase = ((GameObject)opConstructedBase.Value).GetComponent<Base>();
                    }

                    Validate.NotNull(latestBase, "latestBase can not be null");
                    latestCell = latestBase.WorldToGrid(ghost.transform.position);
                }
                
                if (latestCell != default(Int3))
                {
                    cellTransform = latestBase.GetCellObject(latestCell);
                    if (cellTransform != null)
                    {
                        placedPiece = ThrottledBuilder.FindFinishedPiece(cellTransform, id, constructableBase.techType);
                    }
                }

                // This check ensures that the latestCell actually leads us to the correct entity.  The lastTargetBaseOffset is unreliable as the base shape
                // can change which makes the target offset change. It may be possible to fully deprecate lastTargetBaseOffset and only rely on GetClosestCell; 
                // however, there may still be pieces were the ghost base's target offset is authoritative due to incorrect game object positioning.
                if (placedPiece == null)
                {
                    Int3 position = latestBase.WorldToGrid(ghost.gameObject.transform.position);
                    cellTransform = latestBase.GetCellObject(position);       

                    Validate.NotNull(cellTransform, "Unable to find cell transform at " + latestCell);
                    placedPiece = ThrottledBuilder.FindFinishedPiece(cellTransform, id, constructableBase.techType);
                }
                
                Validate.NotNull(placedPiece, $"Could not find finished piece in cell {latestCell}");

                Object.Destroy(ghost);
                NitroxEntity.SetNewId(placedPiece, id);

                BasePieceSpawnProcessor.RunSpawnProcessor(placedPiece.GetComponent<BaseDeconstructable>(), latestBase, latestCell, placedPiece);
            }
            else if (ghost.TryGetComponent(out Constructable constructable))
            {
                FurnitureSpawnProcessor.RunSpawnProcessor(constructable);
            }
            else
            {
                Log.Error($"Found ghost which is neither base piece nor a constructable: {ghost.name}");
            }

            ConstructionCompleted constructionCompleted = new ConstructionCompleted(id, baseId);
            packetSender.Send(constructionCompleted);
        }


        public void DeconstructionBegin(NitroxId id)
        {
            DeconstructionBegin deconstructionBegin = new DeconstructionBegin(id);
            packetSender.Send(deconstructionBegin);
        }

        public void DeconstructionComplete(GameObject gameObject)
        {
            NitroxId id = NitroxEntity.GetId(gameObject);

            DeconstructionCompleted deconstructionCompleted = new DeconstructionCompleted(id);
            packetSender.Send(deconstructionCompleted);

            // When deconstructed, some objects are simply hidden and potentially re-used later (such as windows). 
            // We want to detach the nitrox entity so a new one can potentially be attached layer
            NitroxEntity.RemoveFrom(gameObject);
        }

        public void MetadataChanged(NitroxId pieceId, BasePieceMetadata metadata)
        {
            BasePieceMetadataChanged changePacket = new BasePieceMetadataChanged(pieceId, metadata);
            packetSender.Send(changePacket);
        }
    }
}

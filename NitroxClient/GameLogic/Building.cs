using System.Collections.Generic;
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
        private const float CONSTRUCTION_CHANGE_EVENT_COOLDOWN_PERIOD_SECONDS = 0.10f;

        private readonly IPacketSender packetSender;
        private readonly RotationMetadataFactory rotationMetadataFactory;

        private float timeSinceLastConstructionChangeEvent;

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
            Transform camera = Camera.main.transform;
            Optional<RotationMetadata> rotationMetadata = rotationMetadataFactory.From(baseGhost);

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

            Transform camera = Camera.main.transform;
            BasePiece basePiece = new BasePiece(id, position.ToDto(), rotation.ToDto(), camera.position.ToDto(), camera.rotation.ToDto(), techType.ToDto(), Optional.OfNullable(parentId), true, Optional.Empty);
            PlaceBasePiece placedBasePiece = new PlaceBasePiece(basePiece);
            packetSender.Send(placedBasePiece);
        }

        public void ChangeConstructionAmount(GameObject gameObject, float amount)
        {
            timeSinceLastConstructionChangeEvent += Time.deltaTime;

            if (timeSinceLastConstructionChangeEvent < CONSTRUCTION_CHANGE_EVENT_COOLDOWN_PERIOD_SECONDS)
            {
                return;
            }

            timeSinceLastConstructionChangeEvent = 0.0f;

            NitroxId id = NitroxEntity.GetId(gameObject);

            if (amount < 0.95f) // Construction complete event handled by function below
            {
                ConstructionAmountChanged amountChanged = new ConstructionAmountChanged(id, amount);
                packetSender.Send(amountChanged);
            }
        }

        public void ConstructionComplete(GameObject ghost, Optional<Base> lastTargetBase, Int3 lastTargetBaseOffset)
        {
            NitroxId baseId = null;
            Optional<object> opConstructedBase = TransientLocalObjectManager.Get(TransientObjectType.BASE_GHOST_NEWLY_CONSTRUCTED_BASE_GAMEOBJECT);

            NitroxId id = NitroxEntity.GetId(ghost);

            Log.Info($"Construction complete on {id} {ghost.name}");

            if (opConstructedBase.HasValue)
            {
                GameObject constructedBase = (GameObject)opConstructedBase.Value;
                baseId = NitroxEntity.GetId(constructedBase);
            }

            // For base pieces, we must switch the id from the ghost to the newly constructed piece.
            // Furniture just uses the same game object as the ghost for the final product.
            if (ghost.GetComponent<ConstructableBase>())
            {
                Int3 latestCell = lastTargetBaseOffset;
                Base latestBase = lastTargetBase.HasValue ? lastTargetBase.Value : ((GameObject)opConstructedBase.Value).GetComponent<Base>();
                baseId = NitroxEntity.GetId(latestBase.gameObject);

                Transform cellTransform = latestBase.GetCellObject(latestCell);

                // This check ensures that the latestCell actually leads us to the correct entity.  The lastTargetBaseOffset is unreliable as the base shape
                // can change which makes the target offset change. It may be possible to fully deprecate lastTargetBaseOffset and only rely on GetClosestCell; 
                // however, there may still be pieces were the ghost base's target offset is authoratitive due to incorrect game object positioning.
                if (latestCell == default(Int3) || cellTransform == null)
                {
                    latestBase.GetClosestCell(ghost.transform.position, out latestCell, out Vector3 _, out float _);
                    cellTransform = latestBase.GetCellObject(latestCell);
                    Validate.NotNull(cellTransform, "Unable to find cell transform at " + latestCell);
                }

                GameObject finishedPiece = null;
                
                // There can be multiple objects in a cell (such as a corridor with hatches built into it)
                // we look for a object that is able to be deconstructed that hasn't been tagged yet.
                foreach (Transform child in cellTransform)
                {
                    bool isNewBasePiece = !child.GetComponent<NitroxEntity>() && child.GetComponent<BaseDeconstructable>();

                    // TornacTODO: some parts are colliding with environment so they don't finish placing

                    // The problem is about the NitroxEntity that is not deleted from the ghost object (cf. Constructable_SetState_Patch)
                    // When you destroy an object, rebuilding the same one will make it keep its old NitroxId, which breaks the system
                    if (!isNewBasePiece && child.TryGetComponent(out NitroxEntity nitroxEntity) && id == nitroxEntity.Id)
                    {
                        isNewBasePiece = true;
                    }

                    // TODO: remove these debug lines when merging
                    string printed = $"Found child, isNewBasePiece: {isNewBasePiece} [Name: {child.name}";
                    if (child.TryGetComponent(out NitroxEntity entity))
                    {
                        printed += $", NitroxEntity: {entity.Id}";
                    }
                    if (child.TryGetComponent(out BaseDeconstructable baseDeconstructable))
                    {
                        printed += $", BaseDeconstructable: {baseDeconstructable}";
                    }
                    printed += "]";
                    Log.Debug(printed);

                    if (isNewBasePiece)
                    {
                        finishedPiece = child.gameObject;
                        break;
                    }
                }

                Validate.NotNull(finishedPiece, $"Could not find finished piece in cell {latestCell}");

                Log.Info($"Setting id to finished piece: {finishedPiece.name} {id}");

                Object.Destroy(ghost);
                NitroxEntity.SetNewId(finishedPiece, id);

                BasePieceSpawnProcessor.RunSpawnProcessor(finishedPiece.GetComponent<BaseDeconstructable>(), latestBase, latestCell, finishedPiece);
            }
            else if (ghost.TryGetComponent(out Constructable constructable))
            {
                FurnitureSpawnProcessor.RunSpawnProcessor(constructable);
            }
            else
            {
                Log.Error($"Found ghost which is neither base piece nor a constructable: {ghost.name}");
            }

            Log.Info($"Construction Completed {id} in base {baseId}");

            ConstructionCompleted constructionCompleted = new (id, baseId);
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

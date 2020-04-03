using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Bases.Spawning;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
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

            if (targetBase != null)
            {
                parentBaseId = NitroxEntity.GetId(targetBase.gameObject);
            }
            else if(constructableBase != null)
            {
                Base playerBase = constructableBase.gameObject.GetComponentInParent<Base>();

                if(playerBase != null)
                {
                    parentBaseId = NitroxEntity.GetId(playerBase.gameObject);
                }
            }

            if(parentBaseId == null)
            {
                Base playerBase = baseGhost.gameObject.GetComponentInParent<Base>();

                if (playerBase != null)
                {
                    parentBaseId = NitroxEntity.GetId(playerBase.gameObject);
                }
            }
            
            Vector3 placedPosition = constructableBase.gameObject.transform.position;
            Transform camera = Camera.main.transform;
            Optional<RotationMetadata> rotationMetadata = rotationMetadataFactory.From(baseGhost);

            BasePiece basePiece = new BasePiece(id, placedPosition, quaternion, camera.position, camera.rotation, techType.Model(), Optional.OfNullable(parentBaseId), false, rotationMetadata);
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

                if(playerBase != null)
                {
                    parentId = NitroxEntity.GetId(playerBase.gameObject);
                }
            }

            Transform camera = Camera.main.transform;
            BasePiece basePiece = new BasePiece(id, itemPosition, quaternion, camera.position, camera.rotation, techType.Model(), Optional.OfNullable(parentId), true, Optional.Empty);
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

            Log.Info("Construction complete on " + id + " " + ghost.name);

            if (opConstructedBase.HasValue)
            {
                GameObject constructedBase = (GameObject)opConstructedBase.Value;
                baseId = NitroxEntity.GetId(constructedBase);
            }
            
            // For base pieces, we must switch the id from the ghost to the newly constructed piece.
            // Furniture just uses the same game object as the ghost for the final product.
            if (ghost.GetComponent<ConstructableBase>() != null)
            {
                Int3 latestCell = lastTargetBaseOffset;
                Base latestBase = (lastTargetBase.HasValue) ? lastTargetBase.Value : ((GameObject)opConstructedBase.Value).GetComponent<Base>();

                Transform cellTransform = latestBase.GetCellObject(latestCell);

                // This check ensures that the latestCell actually leads us to the correct entity.  The lastTargetBaseOffset is unreliable as the base shape
                // can change which makes the target offset change. It may be possible to fully deprecate lastTargetBaseOffset and only rely on GetClosestCell; 
                // however, there may still be pieces were the ghost base's target offset is authoratitive due to incorrect game object positioning.
                if (latestCell == default(Int3) || cellTransform == null)
                {
                    Vector3 worldPosition;
                    float distance;

                    latestBase.GetClosestCell(ghost.transform.position, out latestCell, out worldPosition, out distance);
                    cellTransform = latestBase.GetCellObject(latestCell);
                    Validate.NotNull(cellTransform, "Unable to find cell transform at " + latestCell);
                }

                GameObject finishedPiece = null;
                
                // There can be multiple objects in a cell (such as a corridor with hatces built into it)
                // we look for a object that is able to be deconstucted that hasn't been tagged yet.
                foreach (Transform child in cellTransform)
                {
                    bool isNewBasePiece = (child.GetComponent<NitroxEntity>() == null &&
                                           child.GetComponent<BaseDeconstructable>() != null);

                    if (isNewBasePiece)
                    {
                        finishedPiece = child.gameObject;
                        break;
                    }
                }
                
                Validate.NotNull(finishedPiece, "Could not find finished piece in cell " + latestCell);

                Log.Info("Setting id to finished piece: " + finishedPiece.name + " " + id);

                UnityEngine.Object.Destroy(ghost);
                NitroxEntity.SetNewId(finishedPiece, id);

                BasePieceSpawnProcessor customSpawnProcessor = BasePieceSpawnProcessor.From(finishedPiece.GetComponent<BaseDeconstructable>());
                customSpawnProcessor.SpawnPostProcess(latestBase, latestCell, finishedPiece);
            }

            Log.Info("Construction Completed " + id);

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
        }
    }
}

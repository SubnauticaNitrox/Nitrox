using System;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Bases.Spawning.BasePiece;
using NitroxClient.GameLogic.Bases.Spawning.Furniture;
using NitroxClient.GameLogic.InitialSync;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxClient.MonoBehaviours
{
    /**
     * Build events normally can not happen within the same frame as they can cause
     * changes to the surrounding environment.  This class encapsulates logic to 
     * execute build events at a throttled rate of once per frame.  All build logic
     * is contained within this class (it used to be in the individual packet processors)
     * because we want the build logic to be re-usable.
     */
    public class ThrottledBuilder : MonoBehaviour
    {
        public static ThrottledBuilder Main;

        public event EventHandler QueueDrained;
        private BuildThrottlingQueue buildEvents;
        private IPacketSender packetSender;

        public void Start()
        {
            Main = this;
            buildEvents = NitroxServiceLocator.LocateService<BuildThrottlingQueue>();
            packetSender = NitroxServiceLocator.LocateService<IPacketSender>();
        }

        public void Update()
        {
            if (!LargeWorldStreamer.main || !LargeWorldStreamer.main.IsReady() || !LargeWorldStreamer.main.IsWorldSettled())
            {
                return;
            }

            bool queueHadItems = (buildEvents.Count > 0);

            ProcessBuildEventsUntilFrameBlocked();

            if (queueHadItems && buildEvents.Count == 0 && QueueDrained != null)
            {
                QueueDrained(this, EventArgs.Empty);
            }
        }

        private void ProcessBuildEventsUntilFrameBlocked()
        {
            bool processedFrameBlockingEvent = false;
            bool isNextEventFrameBlocked = false;

            while (buildEvents.Count > 0 && !isNextEventFrameBlocked)
            {
                BuildEvent nextEvent = buildEvents.Dequeue();

                try
                {
                    ActionBuildEvent(nextEvent);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error processing buildEvent in ThrottledBuilder");
                }

                if (nextEvent.RequiresFreshFrame())
                {
                    processedFrameBlockingEvent = true;
                }

                isNextEventFrameBlocked = (processedFrameBlockingEvent && buildEvents.NextEventRequiresFreshFrame());
            }
        }

        private void ActionBuildEvent(BuildEvent buildEvent)
        {
            using (packetSender.Suppress<ConstructionAmountChanged>())
            using (packetSender.Suppress<ConstructionCompleted>())
            using (packetSender.Suppress<PlaceBasePiece>())
            using (packetSender.Suppress<DeconstructionBegin>())
            using (packetSender.Suppress<DeconstructionCompleted>())
            using (packetSender.Suppress<BasePieceMetadataChanged>())
            {
                switch (buildEvent)
                {
                    case BasePiecePlacedEvent @event:
                        PlaceBasePiece(@event);
                        break;
                    case ConstructionCompletedEvent completedEvent:
                        ConstructionCompleted(completedEvent);
                        break;
                    case LaterConstructionCompletedEvent laterConstructionCompleted:
                        LaterConstructionCompleted(laterConstructionCompleted);
                        break;
                    case ConstructionAmountChangedEvent changedEvent:
                        ConstructionAmountChanged(changedEvent);
                        break;
                    case DeconstructionBeginEvent beginEvent:
                        DeconstructionBegin(beginEvent);
                        break;
                    case DeconstructionCompletedEvent deconstructionCompletedEvent:
                        DeconstructionCompleted(deconstructionCompletedEvent);
                        break;
                }
            }
        }

        private void PlaceBasePiece(BasePiecePlacedEvent basePiecePlacedBuildEvent)
        {
            BasePiece basePiece = basePiecePlacedBuildEvent.BasePiece;
            GameObject buildPrefab = CraftData.GetBuildPrefab(basePiece.TechType.ToUnity());
            MultiplayerBuilder.OverridePosition = basePiece.ItemPosition.ToUnity();
            MultiplayerBuilder.OverrideQuaternion = basePiece.Rotation.ToUnity();
            MultiplayerBuilder.OverrideTransform = new GameObject().transform;
            MultiplayerBuilder.OverrideTransform.position = basePiece.CameraPosition.ToUnity();
            MultiplayerBuilder.OverrideTransform.rotation = basePiece.CameraRotation.ToUnity();
            MultiplayerBuilder.PlacePosition = basePiece.ItemPosition.ToUnity();
            MultiplayerBuilder.PlaceRotation = basePiece.Rotation.ToUnity();
            MultiplayerBuilder.RotationMetadata = basePiece.RotationMetadata;
            
            GameObject parentBase = null;
            if (basePiece.ParentId.HasValue)
            {
                parentBase = NitroxEntity.GetObjectFrom(basePiece.ParentId.Value).OrNull();
            }

            MultiplayerBuilder.ParentBase = parentBase;
            MultiplayerBuilder.PlaceBasePiece(buildPrefab);
            MultiplayerBuilder.ParentBase = null;

            Constructable constructable;
            GameObject gameObj;

            if (basePiece.IsFurniture)
            {
                SubRoot subRoot = parentBase ? parentBase.GetComponent<SubRoot>() : null;

                gameObj = MultiplayerBuilder.TryPlaceFurniture(subRoot);
                constructable = gameObj.RequireComponentInParent<Constructable>();
            }
            else
            {
                constructable = MultiplayerBuilder.TryPlaceBase(parentBase);
                gameObj = constructable.gameObject;
            }

            if (parentBase && basePiece.IsFurniture)
            {
                gameObj.transform.parent = parentBase.gameObject.transform;
            }

            NitroxEntity.SetNewId(gameObj, basePiece.Id);

            // Manually call start to initialize the object as we may need to interact with it within the same frame.
            constructable.Start();
        }

        private void ConstructionCompleted(ConstructionCompletedEvent constructionCompleted)
        {
            GameObject constructing = NitroxEntity.RequireObjectFrom(constructionCompleted.PieceId);

            // For bases, we need to transfer the GUID off of the ghost and onto the finished piece.
            // Furniture just re-uses the same piece.
            if (constructing.TryGetComponent(out ConstructableBase constructableBase))
            {
                Int3 latestCell = default;
                Base latestBase = null;

                // must fetch BEFORE setState as the BaseGhost gets destroyed
                BaseGhost baseGhost = constructableBase.model.AliveOrNull()?.GetComponent<BaseGhost>();
                if (baseGhost && baseGhost.TargetBase)
                {
                    latestBase = baseGhost.TargetBase;
                    latestCell = latestBase.WorldToGrid(baseGhost.transform.position);
                }

                constructableBase.constructedAmount = 1f;
                constructableBase.SetState(true);
                

                Transform cellTransform;
                GameObject placedPiece = null;

                
                if (!latestBase)
                {
                    Optional<object> opConstructedBase = Get(TransientObjectType.BASE_GHOST_NEWLY_CONSTRUCTED_BASE_GAMEOBJECT);
                    if (opConstructedBase.HasValue)
                    {
                        latestBase = ((GameObject)opConstructedBase.Value).GetComponent<Base>();
                    }

                    Validate.NotNull(latestBase, "latestBase can not be null");
                    latestCell = latestBase!.WorldToGrid(constructing.transform.position);
                }
                
                if (latestCell != default(Int3))
                {
                    cellTransform = latestBase.GetCellObject(latestCell);

                    if (cellTransform)
                    {
                        placedPiece = FindFinishedPiece(cellTransform, constructionCompleted.PieceId, constructableBase.techType);
                    }
                }
                
                if (!placedPiece)
                {
                    Int3 position = latestBase.WorldToGrid(constructableBase.transform.position);
                    cellTransform = latestBase.GetCellObject(position);
                    Validate.NotNull(cellTransform, "Unable to find cell transform at " + position);
                    
                    placedPiece = FindFinishedPiece(cellTransform, constructionCompleted.PieceId, constructableBase.techType);
                }

                Validate.NotNull(placedPiece, $"Could not find placed Piece in cell {latestCell} when constructing {constructionCompleted.PieceId}");
                
                // This destroy instruction must be executed now, else it won't be able to happen in the case the action will have a later completion
                Destroy(constructableBase.gameObject);
                if (BuildingInitialSyncProcessor.LaterConstructionTechTypes.Contains(constructableBase.techType))
                {
                    // We need to transfer these 3 objects to the later completed event
                    Add(TransientObjectType.LATER_CONSTRUCTED_BASE, placedPiece);
                    Add(TransientObjectType.LATER_OBJECT_LATEST_BASE, latestBase);
                    Add(TransientObjectType.LATER_OBJECT_LATEST_CELL, latestCell);
                    return;
                }
                
                FinishConstructionCompleted(placedPiece, latestBase, latestCell, constructionCompleted.PieceId);
            }
            else if (constructing.TryGetComponent(out Constructable constructable))
            {
                constructable.constructedAmount = 1f;
                constructable.SetState(true);

                FurnitureSpawnProcessor.RunSpawnProcessor(constructable);
            }
            else
            {
                Log.Error($"Found ghost which is neither base piece nor a constructable: {constructing.name}");
            }

            if (constructionCompleted.BaseId != null && !NitroxEntity.GetObjectFrom(constructionCompleted.BaseId).HasValue)
            {
                ConfigureNewlyConstructedBase(constructionCompleted.BaseId);
            }
        }

        // There can be multiple objects in a cell (such as a corridor with hatches built into it)
        // we look for a object that is able to be deconstructed that hasn't been tagged yet.
        // NB: The object in question MUST have the expected tech type so that this won't tag a wrong piece (e.g. BaseWaterPark hatches)
        internal static GameObject FindFinishedPiece(Transform cellTransform, NitroxId pieceId, TechType techType)
        {
            foreach (Transform child in cellTransform)
            {
                bool isFinishedPiece = (!child.TryGetComponent(out NitroxEntity entity) && child.TryGetComponent(out BaseDeconstructable baseDeconstructable) && AreSameTechType(baseDeconstructable.recipe, techType) &&
                                        !child.name.Contains("CorridorConnector")) ||
                                        entity?.Id == pieceId;

                if (isFinishedPiece)
                {
                    return child.gameObject;
                }
            }

            return null;
        }

        private static bool AreSameTechType(TechType actual, TechType expected)
        {
            // Normal case
            if (actual.Equals(expected))
            {
                return true;
            }
            // Specificity for this structure
            if (actual.Equals(TechType.BaseConnector) && expected.Equals(TechType.BaseCorridor))
            {
                return true;
            }
            // In certain cases the actual type has one more letter than the expected type
            // e.g. actual = BaseCorridor but expected = BaseCorridorT
            if (expected.ToString().Contains(actual.ToString()))
            {
                return true;
            }
            return false;
        }

        private void LaterConstructionCompleted(LaterConstructionCompletedEvent laterConstructionCompleted)
        {
            GameObject placedPiece = (GameObject)Get(TransientObjectType.LATER_CONSTRUCTED_BASE);
            Base latestBase = (Base)Get(TransientObjectType.LATER_OBJECT_LATEST_BASE);
            Int3 latestCell = (Int3)Get(TransientObjectType.LATER_OBJECT_LATEST_CELL);

            FinishConstructionCompleted(placedPiece, latestBase, latestCell, laterConstructionCompleted.PieceId);

            // And just like at the end of ConstructionCompleted()
            if (laterConstructionCompleted.BaseId != null && !NitroxEntity.GetObjectFrom(laterConstructionCompleted.BaseId).HasValue)
            {
                ConfigureNewlyConstructedBase(laterConstructionCompleted.BaseId);
            }

            Remove(TransientObjectType.LATER_CONSTRUCTED_BASE);
            Remove(TransientObjectType.LATER_OBJECT_LATEST_BASE);
            Remove(TransientObjectType.LATER_OBJECT_LATEST_CELL);
        }

        private void FinishConstructionCompleted(GameObject finishedPiece, Base latestBase, Int3 latestCell, NitroxId pieceId)
        {
            NitroxEntity.SetNewId(finishedPiece, pieceId);
            BasePieceSpawnProcessor.RunSpawnProcessor(finishedPiece.GetComponent<BaseDeconstructable>(), latestBase, latestCell, finishedPiece);
        }

        private void ConfigureNewlyConstructedBase(NitroxId newBaseId)
        {
            Optional<object> opNewlyCreatedBase = Get(TransientObjectType.BASE_GHOST_NEWLY_CONSTRUCTED_BASE_GAMEOBJECT);

            if (opNewlyCreatedBase.HasValue)
            {
                GameObject newlyCreatedBase = (GameObject)opNewlyCreatedBase.Value;
                NitroxEntity.SetNewId(newlyCreatedBase, newBaseId);
            }
            else
            {
                Log.Error("Could not assign new base id as no newly constructed base was found");
            }
        }

        private void ConstructionAmountChanged(ConstructionAmountChangedEvent amountChanged)
        {
            GameObject constructing = NitroxEntity.RequireObjectFrom(amountChanged.Id);
            BaseDeconstructable baseDeconstructable = constructing.GetComponent<BaseDeconstructable>();

            // Bases don't  send a deconstruct being packet.  Instead, we just make sure
            // that if we are changing the amount that we set it into deconstruction mode
            // if it still has a BaseDeconstructable object on it.
            if (baseDeconstructable)
            {
                baseDeconstructable.Deconstruct();
                
                // After we have begun the deconstructing for a base piece, we need to transfer the id
                Optional<object> opGhost = Get(TransientObjectType.LATEST_DECONSTRUCTED_BASE_PIECE_GHOST);

                if (opGhost.HasValue && opGhost.Value is Component component)
                {
                    NitroxEntity.SetNewId(component.gameObject, amountChanged.Id);
                    Destroy(constructing);
                }
                else
                {
                    Log.Error($"Could not find newly created ghost to set deconstructed id {amountChanged.Id}");
                }
            }
            else
            {
                Constructable constructable = constructing.GetComponentInChildren<Constructable>();
                constructable.constructedAmount = amountChanged.Amount;
                constructable.UpdateMaterial();
            }
        }

        private void DeconstructionBegin(DeconstructionBeginEvent begin)
        {
            GameObject deconstructing = NitroxEntity.RequireObjectFrom(begin.PieceId);
            Constructable constructable = deconstructing.RequireComponent<Constructable>();

            constructable.SetState(false, false);
        }

        private void DeconstructionCompleted(DeconstructionCompletedEvent completed)
        {
            GameObject deconstructing = NitroxEntity.RequireObjectFrom(completed.PieceId);
            Destroy(deconstructing);
        }
    }
}

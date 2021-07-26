using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using System;
using System.Reflection;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;
using NitroxModel_Subnautica.Helper;
using NitroxModel.DataStructures;
using NitroxClient.GameLogic.Bases.Spawning;

namespace NitroxClient.MonoBehaviours
{
    /**
     * Build events normally can not happen within the same frame as they can cause
     * changes to the surrounding environment.  This class encapsulates logic to 
     * execute build events at a throttled rate of once per frame.  All build logic
     * is contained within this class (it used to be in the individual packet processors)
     * because we want the build logic to be re-useable.
     */
    public class ThrottledBuilder : MonoBehaviour
    {
        public static ThrottledBuilder main;

        public event EventHandler QueueDrained;
        private BuildThrottlingQueue buildEvents;
        private IPacketSender packetSender;

        public void Start()
        {
            main = this;
            buildEvents = NitroxServiceLocator.LocateService<BuildThrottlingQueue>();
            packetSender = NitroxServiceLocator.LocateService<IPacketSender>();
        }

        public void Update()
        {
            if(LargeWorldStreamer.main == null || !LargeWorldStreamer.main.IsReady() || !LargeWorldStreamer.main.IsWorldSettled())
            {
                return;
            }

            bool queueHadItems = (buildEvents.Count > 0);

            ProcessBuildEventsUntilFrameBlocked();

            if(queueHadItems && buildEvents.Count == 0 && QueueDrained != null)
            {
                QueueDrained(this, new EventArgs());
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
                    Log.Error("Error processing buildEvent in ThrottledBuilder" + ex);
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
            if (buildEvent is BasePiecePlacedEvent)
            {
                PlaceBasePiece((BasePiecePlacedEvent)buildEvent);
            }
            else if (buildEvent is ConstructionCompletedEvent)
            {
                ConstructionCompleted((ConstructionCompletedEvent)buildEvent);
            }
            else if (buildEvent is ConstructionAmountChangedEvent)
            {
                ConstructionAmountChanged((ConstructionAmountChangedEvent)buildEvent);
            }
            else if (buildEvent is DeconstructionBeginEvent)
            {
                DeconstructionBegin((DeconstructionBeginEvent)buildEvent);
            }
            else if (buildEvent is DeconstructionCompletedEvent)
            {
                DeconstructionCompleted((DeconstructionCompletedEvent)buildEvent);
            }
        }

        private void PlaceBasePiece(BasePiecePlacedEvent basePiecePlacedBuildEvent)
        {
            Log.Info("BuildBasePiece " + basePiecePlacedBuildEvent.BasePiece.Id + " type: " + basePiecePlacedBuildEvent.BasePiece.TechType + " parentId: " + basePiecePlacedBuildEvent.BasePiece.ParentId.OrElse(null));
            BasePiece basePiece = basePiecePlacedBuildEvent.BasePiece;
            GameObject buildPrefab = CraftData.GetBuildPrefab(basePiece.TechType.Enum());
            MultiplayerBuilder.overridePosition = basePiece.ItemPosition;
            MultiplayerBuilder.overrideQuaternion = basePiece.Rotation;
            MultiplayerBuilder.overrideTransform = new GameObject().transform;
            MultiplayerBuilder.overrideTransform.position = basePiece.CameraPosition;
            MultiplayerBuilder.overrideTransform.rotation = basePiece.CameraRotation;
            MultiplayerBuilder.placePosition = basePiece.ItemPosition;
            MultiplayerBuilder.placeRotation = basePiece.Rotation;
            MultiplayerBuilder.rotationMetadata = basePiece.RotationMetadata;
            MultiplayerBuilder.Begin(buildPrefab);

            GameObject parentBase = null;

            if (basePiece.ParentId.HasValue)
            {
                parentBase = NitroxEntity.GetObjectFrom(basePiece.ParentId.Value).OrElse(null);
            }
            
            Constructable constructable;
            GameObject gameObject;

            if (basePiece.IsFurniture)
            {
                SubRoot subRoot = (parentBase != null) ? parentBase.GetComponent<SubRoot>() : null;
                                
                gameObject = MultiplayerBuilder.TryPlaceFurniture(subRoot);
                constructable = gameObject.RequireComponentInParent<Constructable>();
            }
            else
            {
                constructable = MultiplayerBuilder.TryPlaceBase(parentBase);
                gameObject = constructable.gameObject;
            }

            NitroxEntity.SetNewId(gameObject, basePiece.Id);
            
            /**
             * Manually call start to initialize the object as we may need to interact with it within the same frame.
             */
            MethodInfo startCrafting = typeof(Constructable).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
            Validate.NotNull(startCrafting);
            startCrafting.Invoke(constructable, new object[] { });
        }

        private void ConstructionCompleted(ConstructionCompletedEvent constructionCompleted)
        {
            GameObject constructing = NitroxEntity.RequireObjectFrom(constructionCompleted.PieceId);

            ConstructableBase constructableBase = constructing.GetComponent<ConstructableBase>();

            // For bases, we need to transfer the GUID off of the ghost and onto the finished piece.
            // Furniture just re-uses the same piece.
            if (constructableBase)
            {
                Int3 latestCell = default(Int3);
                Base latestBase = null;

                // must fetch BEFORE setState or else the BaseGhost gets destroyed
                BaseGhost baseGhost = constructing.GetComponentInChildren<BaseGhost>();

                if(baseGhost)
                {
                    latestCell = baseGhost.TargetOffset;
                    latestBase = baseGhost.TargetBase;
                }

                constructableBase.constructedAmount = 1f;
                constructableBase.SetState(true, true);
                
                if(latestBase == null)
                {
                    Optional<object> opConstructedBase = TransientLocalObjectManager.Get(TransientObjectType.BASE_GHOST_NEWLY_CONSTRUCTED_BASE_GAMEOBJECT);
                    latestBase = ((GameObject)opConstructedBase.Value).GetComponent<Base>();
                    Validate.NotNull(latestBase, "latestBase can not be null");
                }
                
                Transform cellTransform = latestBase.GetCellObject(latestCell);

                if (latestCell == default(Int3) || cellTransform == null)
                {
                    Vector3 worldPosition;
                    float distance;

                    latestBase.GetClosestCell(constructing.gameObject.transform.position, out latestCell, out worldPosition, out distance);
                    cellTransform = latestBase.GetCellObject(latestCell);
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

                Log.Info("Construction completed on a base piece: " + constructionCompleted.PieceId + " " + finishedPiece.name);

                UnityEngine.Object.Destroy(constructableBase.gameObject);
                NitroxEntity.SetNewId(finishedPiece, constructionCompleted.PieceId);
                
                BasePieceSpawnProcessor customSpawnProcessor = BasePieceSpawnProcessor.From(finishedPiece.GetComponent<BaseDeconstructable>());
                customSpawnProcessor.SpawnPostProcess(latestBase, latestCell, finishedPiece);
            }
            else
            {
                Constructable constructable = constructing.GetComponent<Constructable>();
                constructable.constructedAmount = 1f;
                constructable.SetState(true, true);

                Log.Info("Construction completed on a piece of furniture: " + constructionCompleted.PieceId + " " + constructable.gameObject.name);
            }
            
            if (constructionCompleted.BaseId != null && !NitroxEntity.GetObjectFrom(constructionCompleted.BaseId).HasValue)
            {
                Log.Info("Creating base: " + constructionCompleted.BaseId);
                ConfigureNewlyConstructedBase(constructionCompleted.BaseId);
            }
        }

        private void ConfigureNewlyConstructedBase(NitroxId newBaseId)
        {
            Optional<object> opNewlyCreatedBase = TransientLocalObjectManager.Get(TransientLocalObjectManager.TransientObjectType.BASE_GHOST_NEWLY_CONSTRUCTED_BASE_GAMEOBJECT);

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
            Log.Info("Processing ConstructionAmountChanged " + amountChanged.Id + " " + amountChanged.Amount);

            GameObject constructing = NitroxEntity.RequireObjectFrom(amountChanged.Id);
            BaseDeconstructable baseDeconstructable = constructing.GetComponent<BaseDeconstructable>();

            // Bases don't  send a deconstruct being packet.  Instead, we just make sure
            // that if we are changing the amount that we set it into deconstruction mode
            // if it still has a BaseDeconstructable object on it.
            if (baseDeconstructable != null)
            {
                baseDeconstructable.Deconstruct();

                // After we have begun the deconstructing for a base piece, we need to transfer the id
                Optional<object> opGhost = TransientLocalObjectManager.Get(TransientObjectType.LATEST_DECONSTRUCTED_BASE_PIECE);

                if (opGhost.HasValue)
                {
                    GameObject ghost = (GameObject)opGhost.Value;
                    Destroy(constructing);
                    NitroxEntity.SetNewId(ghost, amountChanged.Id);
                }
                else
                {
                    Log.Info("Could not find newly created ghost to set deconstructed id ");
                }
            }
            else
            {
                Constructable constructable = constructing.GetComponentInChildren<Constructable>();
                constructable.constructedAmount = amountChanged.Amount;

                using (packetSender.Suppress<ConstructionAmountChanged>())
                {
                    constructable.Construct();
                }
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
            UnityEngine.Object.Destroy(deconstructing);
        }
    }
}

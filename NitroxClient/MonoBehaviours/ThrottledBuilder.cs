using System;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Bases.Spawning.BasePiece;
using NitroxClient.GameLogic.Bases.Spawning.Furniture;
using NitroxClient.GameLogic.Helper;
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
            if (LargeWorldStreamer.main == null || !LargeWorldStreamer.main.IsReady() || !LargeWorldStreamer.main.IsWorldSettled())
            {
                return;
            }

            bool queueHadItems = (buildEvents.Count > 0);

            ProcessBuildEventsUntilFrameBlocked();

            if (queueHadItems && buildEvents.Count == 0 && QueueDrained != null)
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
                    case BasePiecePlacedEvent:
                        PlaceBasePiece((BasePiecePlacedEvent)buildEvent);
                        break;
                    case ConstructionCompletedEvent:
                        ConstructionCompleted((ConstructionCompletedEvent)buildEvent);
                        break;
                    case LaterConstructionCompletedEvent:
                        LaterConstructionCompleted((LaterConstructionCompletedEvent)buildEvent);
                        break;
                    case ConstructionAmountChangedEvent:
                        ConstructionAmountChanged((ConstructionAmountChangedEvent)buildEvent);
                        break;
                    case DeconstructionBeginEvent:
                        DeconstructionBegin((DeconstructionBeginEvent)buildEvent);
                        break;
                    case DeconstructionCompletedEvent:
                        DeconstructionCompleted((DeconstructionCompletedEvent)buildEvent);
                        break;
                }
            }
        }

        private void PlaceBasePiece(BasePiecePlacedEvent basePiecePlacedBuildEvent)
        {
            Log.Debug($"BuildBasePiece - {basePiecePlacedBuildEvent.BasePiece.Id} type: {basePiecePlacedBuildEvent.BasePiece.TechType} parentId: {basePiecePlacedBuildEvent.BasePiece.ParentId.OrElse(null)}");

            BasePiece basePiece = basePiecePlacedBuildEvent.BasePiece;
            GameObject buildPrefab = CraftData.GetBuildPrefab(basePiece.TechType.ToUnity());
            MultiplayerBuilder.overridePosition = basePiece.ItemPosition.ToUnity();
            MultiplayerBuilder.overrideQuaternion = basePiece.Rotation.ToUnity();
            MultiplayerBuilder.overrideTransform = new GameObject().transform;
            MultiplayerBuilder.overrideTransform.position = basePiece.CameraPosition.ToUnity();
            MultiplayerBuilder.overrideTransform.rotation = basePiece.CameraRotation.ToUnity();
            MultiplayerBuilder.placePosition = basePiece.ItemPosition.ToUnity();
            MultiplayerBuilder.placeRotation = basePiece.Rotation.ToUnity();
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

            if (parentBase != null && basePiece.IsFurniture)
            {
                gameObject.transform.parent = parentBase.gameObject.transform;
            }

            NitroxEntity.SetNewId(gameObject, basePiece.Id);

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
                Int3 latestCell = default(Int3);
                Base latestBase = null;

                // must fetch BEFORE setState or else the BaseGhost gets destroyed
                BaseGhost baseGhost = constructing.GetComponentInChildren<BaseGhost>();

                if (baseGhost)
                {
                    latestCell = baseGhost.TargetOffset;
                    latestBase = baseGhost.TargetBase;
                }

                constructableBase.constructedAmount = 1f;
                constructableBase.SetState(true, true);
                
                if (!latestBase)
                {
                    Optional<object> opConstructedBase = TransientLocalObjectManager.Get(TransientObjectType.BASE_GHOST_NEWLY_CONSTRUCTED_BASE_GAMEOBJECT);
                    latestBase = ((GameObject)opConstructedBase.Value).GetComponent<Base>();
                    Validate.NotNull(latestBase, "latestBase can not be null");
                }

                Transform cellTransform = latestBase.GetCellObject(latestCell);

                if (latestCell == default(Int3) || !cellTransform)
                {
                    latestBase.GetClosestCell(constructing.gameObject.transform.position, out latestCell, out _, out _);
                    cellTransform = latestBase.GetCellObject(latestCell);

                    Validate.NotNull(cellTransform, $"Must have a cell transform, one not found near {constructing.gameObject.transform.position} for latestCell {latestCell}");
                }

                // This destroy instruction must be executed now, else it won't be able to happen in the case the action will have a later completion
                Destroy(constructableBase.gameObject);
                if (BuildingInitialSyncProcessor.LaterConstructionTechTypes.Contains(constructableBase.techType))
                {
                    Log.Debug($"First part of construction completed on a base piece: {constructionCompleted.PieceId}");
                    // We need to transfer these 3 objects to the later completed event
                    Add(TransientObjectType.LATER_CONSTRUCTED_BASE, cellTransform);
                    Add(TransientObjectType.LATER_OBJECT_LATEST_BASE, latestBase);
                    Add(TransientObjectType.LATER_OBJECT_LATEST_CELL, latestCell);
                    return;
                }

                FinishConstructionCompleted(cellTransform, latestBase, latestCell, constructionCompleted.PieceId);
            }
            else if (constructing.TryGetComponent(out Constructable constructable))
            {
                constructable.constructedAmount = 1f;
                constructable.SetState(true, true);

                FurnitureSpawnProcessor.RunSpawnProcessor(constructable);

                Log.Debug($"Construction completed on a piece of furniture: {constructionCompleted.PieceId} {constructable.gameObject.name}");
            }
            else
            {
                Log.Error($"Found ghost which is neither base piece nor a constructable: {constructing.name}");
            }

            if (constructionCompleted.BaseId != null && !NitroxEntity.GetObjectFrom(constructionCompleted.BaseId).HasValue)
            {
                Log.Debug($"Creating base: {constructionCompleted.BaseId}");
                ConfigureNewlyConstructedBase(constructionCompleted.BaseId);
            }
        }

        private void LaterConstructionCompleted(LaterConstructionCompletedEvent laterConstructionCompleted)
        {
            Transform cellTransform = (Transform)Get(TransientObjectType.LATER_CONSTRUCTED_BASE);
            Base latestBase = (Base)Get(TransientObjectType.LATER_OBJECT_LATEST_BASE);
            Int3 latestCell = (Int3)Get(TransientObjectType.LATER_OBJECT_LATEST_CELL);

            FinishConstructionCompleted(cellTransform, latestBase, latestCell, laterConstructionCompleted.PieceId);

            // And just like at the end of ConstructionCompleted()
            if (laterConstructionCompleted.BaseId != null && !NitroxEntity.GetObjectFrom(laterConstructionCompleted.BaseId).HasValue)
            {
                Log.Debug($"Creating base: {laterConstructionCompleted.BaseId}");
                ConfigureNewlyConstructedBase(laterConstructionCompleted.BaseId);
            }

            Remove(TransientObjectType.LATER_CONSTRUCTED_BASE);
            Remove(TransientObjectType.LATER_OBJECT_LATEST_BASE);
            Remove(TransientObjectType.LATER_OBJECT_LATEST_CELL);
        }

        private void FinishConstructionCompleted(Transform cellTransform, Base latestBase, Int3 latestCell, NitroxId pieceId)
        {
            GameObject finishedPiece = null;

            // There can be multiple objects in a cell (such as a corridor with hatches built into it)
            // we look for a object that is able to be deconstructed that hasn't been tagged yet.
            foreach (Transform child in cellTransform)
            {
                bool isNewBasePiece = !child.GetComponent<NitroxEntity>() && child.GetComponent<BaseDeconstructable>();

                // TODO: remove these debug lines when merging
                string printed = $"Found child : [Name: {child.name}";
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

                if (!isNewBasePiece && child.TryGetComponent(out NitroxEntity nitroxEntity) && pieceId == nitroxEntity.Id)
                {
                    Log.Debug("[ThrottledBuilder] TEST FOUND");
                    isNewBasePiece = true;
                }

                if (isNewBasePiece)
                {
                    finishedPiece = child.gameObject;
                    break;
                }
            }

            Validate.NotNull(finishedPiece, $"Could not find finished piece in cell {latestCell} when constructing {pieceId}");

            Log.Debug($"Construction completed on a base piece: {pieceId} {finishedPiece.name}");

            NitroxEntity.SetNewId(finishedPiece, pieceId);

            BasePieceSpawnProcessor.RunSpawnProcessor(finishedPiece.GetComponent<BaseDeconstructable>(), latestBase, latestCell, finishedPiece);
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
            Log.Debug($"Processing ConstructionAmountChanged {amountChanged.Id} {amountChanged.Amount}");

            GameObject constructing = NitroxEntity.RequireObjectFrom(amountChanged.Id);
            BaseDeconstructable baseDeconstructable = constructing.GetComponent<BaseDeconstructable>();

            // Bases don't  send a deconstruct being packet.  Instead, we just make sure
            // that if we are changing the amount that we set it into deconstruction mode
            // if it still has a BaseDeconstructable object on it.
            if (baseDeconstructable != null)
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
                constructable.Construct();
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

using System;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Bases.Spawning.BasePiece;
using NitroxClient.GameLogic.Bases.Spawning.Furniture;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
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
        private readonly MethodInfo constructableStartMethod = typeof(Constructable).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);

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
            Log.Debug($"BuildBasePiece - {basePiecePlacedBuildEvent.BasePiece} type: {basePiecePlacedBuildEvent.BasePiece.TechType} parentId: {basePiecePlacedBuildEvent.BasePiece.ParentId.OrNull()}");

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
                parentBase = NitroxEntity.GetObjectFrom(basePiece.ParentId.Value).OrNull();
            }

            Constructable constructable;
            GameObject gameObj;

            if (basePiece.IsFurniture)
            {
                SubRoot subRoot = (parentBase != null) ? parentBase.GetComponent<SubRoot>() : null;

                gameObj = MultiplayerBuilder.TryPlaceFurniture(subRoot);
                constructable = gameObj.RequireComponentInParent<Constructable>();
            }
            else
            {
                constructable = MultiplayerBuilder.TryPlaceBase(parentBase);
                gameObj = constructable.gameObject;
            }

            if (parentBase != null && basePiece.IsFurniture)
            {
                gameObj.transform.parent = parentBase.gameObject.transform;
            }

            NitroxEntity.SetNewId(gameObj, basePiece.Id);

            // Manually call start to initialize the object as we may need to interact with it within the same frame.
            Validate.NotNull(constructableStartMethod);
            constructableStartMethod.Invoke(constructable, Array.Empty<object>());
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
                Base.Face lastFace = default;

                // must fetch BEFORE setState as the BaseGhost gets destroyed
                BaseGhost baseGhost = constructableBase.model.AliveOrNull()?.GetComponent<BaseGhost>();
                if (baseGhost != null)
                {
                    latestBase = baseGhost.TargetBase;
                    latestCell = latestBase.AliveOrNull()?.WorldToGrid(baseGhost.transform.position) ?? latestCell;

                    lastFace = baseGhost switch
                    {
                        BaseAddFaceGhost { anchoredFace: { } } baseAddFaceGhost => baseAddFaceGhost.anchoredFace.Value,
                        BaseAddModuleGhost { anchoredFace: { } } baseAddModuleGhost => baseAddModuleGhost.anchoredFace.Value,
                        _ => lastFace,
                    };
                }

                constructableBase.SetState(true, true);

                if (!latestBase)
                {
                    Optional<object> opConstructedBase = Get(TransientObjectType.BASE_GHOST_NEWLY_CONSTRUCTED_BASE_GAMEOBJECT);
                    if (opConstructedBase.HasValue)
                    {
                        latestBase = ((GameObject)opConstructedBase.Value).GetComponent<Base>();
                    }

                    Validate.NotNull(latestBase, "latestBase can not be null");
                    latestCell = latestBase.WorldToGrid(constructing.transform.position);
                }

                Transform cellTransform;
                GameObject finishedPiece = null;

                // This is the way the game uses find the final object if the built part is an internal face part like a window, reinforcement, bio-reactor, etc.
                if (lastFace != default(Base.Face))
                {
                    cellTransform = latestBase.GetCellObject(lastFace.cell);
                    Validate.NotNull(cellTransform, "Unable to find cell transform at " + lastFace.cell);

                    Log.Debug($"Looking for {constructing.name} in cell {lastFace.cell}");
                    finishedPiece = FindFinishedPiece(cellTransform);
                }

                if (!finishedPiece)
                {
                    if (latestCell != default(Int3))
                    {
                        cellTransform = latestBase.GetCellObject(latestCell);
                        if (cellTransform != null)
                        {
                            Log.Debug($"Looking for {constructing.name} in cell {latestCell}");
                            finishedPiece = FindFinishedPiece(cellTransform);
                        }
                    }

                    // This check ensures that the latestCell actually leads us to the correct entity.  The lastTargetBaseOffset is unreliable as the base shape
                    // can change which makes the target offset change. It may be possible to fully deprecate lastTargetBaseOffset and only rely on GetClosestCell; 
                    // however, there may still be pieces were the ghost base's target offset is authoratitive due to incorrect game object positioning.
                    if (!finishedPiece)
                    {
                        latestCell = latestBase.WorldToGrid(constructing.transform.position);
                        cellTransform = latestBase.GetCellObject(latestCell);

                        Validate.NotNull(cellTransform, $"Must have a cell transform, one not found near {constructing.gameObject.transform.position} for latestCell {latestCell}");
                        Log.Debug($"Looking for {constructing.name} in cell {latestCell}");
                        finishedPiece = FindFinishedPiece(cellTransform);
                    }
                }
                Validate.NotNull(finishedPiece, $"Could not find finished piece in cell {latestCell} when constructing {constructionCompleted.PieceId}");

                Log.Debug($"Construction completed on a base piece: {constructionCompleted.PieceId} {finishedPiece.name}");

                Destroy(constructableBase.gameObject);
                NitroxEntity.SetNewId(finishedPiece, constructionCompleted.PieceId);

                BasePieceSpawnProcessor.RunSpawnProcessor(finishedPiece.GetComponent<BaseDeconstructable>(), latestBase, latestCell, finishedPiece);
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
        
        // There can be multiple objects in a cell (such as a corridor with hatches built into it)
        // we look for a object that is able to be deconstructed that hasn't been tagged yet.
        private static GameObject FindFinishedPiece(Transform cellTransform)
        {
            foreach (Transform child in cellTransform)
            {
                bool isNewBasePiece = !child.TryGetComponent(out NitroxEntity _) && child.GetComponent<BaseDeconstructable>() && !child.name.Contains("CorridorConnector");
                if (isNewBasePiece)
                {
                    return child.gameObject;
                }
            }

            return null;
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

                if (opGhost.HasValue)
                {
                    GameObject ghost = (GameObject)opGhost.Value;
                    Destroy(constructing);
                    NitroxEntity.SetNewId(ghost, amountChanged.Id);
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
            Destroy(deconstructing);
        }
    }
}

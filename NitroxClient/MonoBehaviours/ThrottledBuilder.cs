using System;
using System.Linq;
using System.Reflection;
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
        public static ThrottledBuilder Instance;

        public event EventHandler QueueDrained;
        private BuildThrottlingQueue buildEvents;
        private IPacketSender packetSender;

        public void Start()
        {
            Instance = this;
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

            if (queueHadItems && buildEvents.Count == 0)
            {
                QueueDrained?.Invoke(this, new EventArgs());
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
            switch (buildEvent)
            {
                case BasePiecePlacedEvent basePiecePlacedEvent:
                    PlaceBasePiece(basePiecePlacedEvent);
                    break;
                case ConstructionCompletedEvent constructionCompletedEvent:
                    ConstructionCompleted(constructionCompletedEvent);
                    break;
                case ConstructionAmountChangedEvent constructionAmountChangedEvent:
                    ConstructionAmountChanged(constructionAmountChangedEvent);
                    break;
                case DeconstructionBeginEvent deconstructionBeginEvent:
                    DeconstructionBegin(deconstructionBeginEvent);
                    break;
                case DeconstructionCompletedEvent deconstructionCompletedEvent:
                    DeconstructionCompleted(deconstructionCompletedEvent);
                    break;
            }
        }

        private static void PlaceBasePiece(BasePiecePlacedEvent basePiecePlacedBuildEvent)
        {
            Log.Info($"BuildBasePiece {basePiecePlacedBuildEvent.BasePiece.Id} type: {basePiecePlacedBuildEvent.BasePiece.TechType} parentId: {basePiecePlacedBuildEvent.BasePiece.Transform.Parent.Id}");
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
                SubRoot subRoot = parentBase ? parentBase.GetComponent<SubRoot>() : null;

                gameObject = MultiplayerBuilder.TryPlaceFurniture(subRoot);
                constructable = gameObject.RequireComponentInParent<Constructable>();
            }
            else
            {
                constructable = MultiplayerBuilder.TryPlaceBase(parentBase);
                gameObject = constructable.gameObject;
            }

            NitroxEntity.SetNewId(gameObject, basePiece.Id);

            //Manually call start to initialize the object as we may need to interact with it within the same frame.
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
                Int3 latestCell = default;
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

                if (!latestBase)
                {
                    Optional<object> opConstructedBase = TransientLocalObjectManager.Get(TransientLocalObjectManager.TransientObjectType.BASE_GHOST_NEWLY_CONSTRUCTED_BASE_GAMEOBJECT);
                    latestBase = ((GameObject)opConstructedBase.Value).GetComponent<Base>();
                    Validate.NotNull(latestBase, "latestBase can not be null");
                }
                
                Transform cellTransform = latestBase.GetCellObject(latestCell);

                if (latestCell == default(Int3) || !cellTransform)
                {
                    latestBase.GetClosestCell(constructing.gameObject.transform.position, out latestCell, out Vector3 _, out float _);
                    cellTransform = latestBase.GetCellObject(latestCell);
                }

                // There can be multiple objects in a cell (such as a corridor with hatches built into it)
                // we look for a object that is able to be deconstucted that hasn't been tagged yet.
                GameObject finishedPiece = (from Transform child in cellTransform let isNewBasePiece = !child.GetComponent<NitroxEntity>() && child.GetComponent<BaseDeconstructable>() where isNewBasePiece select child.gameObject).FirstOrDefault();

                Validate.NotNull(finishedPiece, "Could not find finished piece in cell " + latestCell);

                Log.Info($"Construction completed on a base piece: {constructionCompleted.PieceId} {finishedPiece.name}");

                Destroy(constructableBase.gameObject);
                NitroxEntity.SetNewId(finishedPiece, constructionCompleted.PieceId);
                
                BasePieceSpawnProcessor customSpawnProcessor = BasePieceSpawnProcessor.From(finishedPiece.GetComponent<BaseDeconstructable>());
                customSpawnProcessor.SpawnPostProcess(latestBase, latestCell, finishedPiece);
            }
            else
            {
                Constructable constructable = constructing.GetComponent<Constructable>();
                constructable.constructedAmount = 1f;
                constructable.SetState(true, true);

                Log.Info($"Construction completed on a piece of furniture: {constructionCompleted.PieceId} {constructable.gameObject.name}");
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
            Log.Info($"Processing ConstructionAmountChanged {amountChanged.Id} {amountChanged.Amount}");

            GameObject constructing = NitroxEntity.RequireObjectFrom(amountChanged.Id);
            BaseDeconstructable baseDeconstructable = constructing.GetComponent<BaseDeconstructable>();

            // Bases don't  send a deconstruct being packet.  Instead, we just make sure
            // that if we are changing the amount that we set it into deconstruction mode
            // if it still has a BaseDeconstructable object on it.
            if (baseDeconstructable)
            {
                baseDeconstructable.Deconstruct();

                // After we have begun the deconstructing for a base piece, we need to transfer the id
                Optional<object> opGhost = TransientLocalObjectManager.Get(TransientLocalObjectManager.TransientObjectType.LATEST_DECONSTRUCTED_BASE_PIECE);

                if (opGhost.HasValue)
                {
                    GameObject ghost = (GameObject)opGhost.Value;
                    Destroy(constructing);
                    NitroxEntity.SetNewId(ghost, amountChanged.Id);
                }
                else
                {
                    Log.Info("Could not find newly created ghost to set deconstructed id");
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
            Destroy(deconstructing);
        }
    }
}

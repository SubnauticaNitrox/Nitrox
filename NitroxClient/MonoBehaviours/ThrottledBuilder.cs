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
        private BuildThrottlingQueue buildEvents;
        private IPacketSender packetSender;

        public void Start()
        {
            buildEvents = NitroxServiceLocator.LocateService<BuildThrottlingQueue>();
            packetSender = NitroxServiceLocator.LocateService<IPacketSender>();
        }

        public void Update()
        {
            if(LargeWorldStreamer.main == null || !LargeWorldStreamer.main.IsReady() || !LargeWorldStreamer.main.IsWorldSettled())
            {
                return;
            }

            Optional<BuildEvent> opBuildEvent = buildEvents.GetNextPendingEvent();

            if(opBuildEvent.IsPresent())
            {
                Log.Info("ThrottledBuilder actioning event " + opBuildEvent.Get().GetType());

                ActionBuildEvent(opBuildEvent.Get());
            }
        }

        private void ActionBuildEvent(BuildEvent buildEvent)
        {
            if (buildEvent is BasePiecePlacedEvent)
            {
                BuildBasePiece((BasePiecePlacedEvent)buildEvent);
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

        private void BuildBasePiece(BasePiecePlacedEvent basePiecePlacedBuildEvent)
        {
            BasePiece basePiece = basePiecePlacedBuildEvent.BasePiece;
            GameObject buildPrefab = CraftData.GetBuildPrefab(basePiece.TechType);
            MultiplayerBuilder.overridePosition = basePiece.ItemPosition;
            MultiplayerBuilder.overrideQuaternion = basePiece.Rotation;
            MultiplayerBuilder.overrideTransform = new GameObject().transform;
            MultiplayerBuilder.overrideTransform.position = basePiece.CameraPosition;
            MultiplayerBuilder.overrideTransform.rotation = basePiece.CameraRotation;
            MultiplayerBuilder.placePosition = basePiece.ItemPosition;
            MultiplayerBuilder.placeRotation = basePiece.Rotation;
            MultiplayerBuilder.Begin(buildPrefab);

            Optional<GameObject> parentBase = (basePiece.ParentBaseGuid.IsPresent()) ? GuidHelper.GetObjectFrom(basePiece.ParentBaseGuid.Get()) : Optional<GameObject>.Empty();

            ConstructableBase constructableBase = MultiplayerBuilder.TryPlaceBase(parentBase);
            GuidHelper.SetNewGuid(constructableBase.gameObject, basePiece.Guid);

            /**
             * Manually call start to initialize the object as we may need to interact with it within the same frame.
             */
            MethodInfo startCrafting = typeof(Constructable).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
            Validate.NotNull(startCrafting);
            startCrafting.Invoke(constructableBase, new object[] { });
        }

        private void ConstructionCompleted(ConstructionCompletedEvent constructionCompleted)
        {
            GameObject constructing = GuidHelper.RequireObjectFrom(constructionCompleted.Guid);
            Constructable constructable = constructing.GetComponent<Constructable>();
            constructable.constructedAmount = 1f;
            constructable.SetState(true, true);

            if (constructionCompleted.NewBaseCreatedGuid.IsPresent())
            {
                string newBaseGuid = constructionCompleted.NewBaseCreatedGuid.Get();
                ConfigureNewlyConstructedBase(newBaseGuid);
            }
        }

        private void ConfigureNewlyConstructedBase(string newBaseGuid)
        {
            Optional<object> opNewlyCreatedBase = TransientLocalObjectManager.Get(TransientLocalObjectManager.TransientObjectType.BASE_GHOST_NEWLY_CONSTRUCTED_BASE_GAMEOBJECT);

            if (opNewlyCreatedBase.IsPresent())
            {
                GameObject newlyCreatedBase = (GameObject)opNewlyCreatedBase.Get();
                GuidHelper.SetNewGuid(newlyCreatedBase, newBaseGuid);
            }
            else
            {
                Log.Error("Could not assign new base guid as no newly constructed base was found");
            }
        }

        private void ConstructionAmountChanged(ConstructionAmountChangedEvent amountChanged)
        {
            Log.Debug("Processing ConstructionAmountChanged " + amountChanged.Guid + " " + amountChanged.Amount);

            GameObject constructing = GuidHelper.RequireObjectFrom(amountChanged.Guid);
            Constructable constructable = constructing.GetComponent<Constructable>();
            constructable.constructedAmount = amountChanged.Amount;

            using (packetSender.Suppress<ConstructionAmountChanged>())
            {
                constructable.Construct();
            }
        }

        private void DeconstructionBegin(DeconstructionBeginEvent begin)
        {
            GameObject deconstructing = GuidHelper.RequireObjectFrom(begin.Guid);
            Constructable constructable = deconstructing.RequireComponent<Constructable>();

            constructable.SetState(false, false);
        }

        private void DeconstructionCompleted(DeconstructionCompletedEvent completed)
        {
            GameObject deconstructing = GuidHelper.RequireObjectFrom(completed.Guid);
            UnityEngine.Object.Destroy(deconstructing);
        }
    }
}

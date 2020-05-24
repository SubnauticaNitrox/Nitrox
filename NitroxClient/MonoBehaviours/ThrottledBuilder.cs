using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxModel.Core;
using NitroxModel.Logger;
using System;
using UnityEngine;
using NitroxClient.GameLogic;


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
            if (buildEvent is ConstructionBeginEvent)
            {
                ConstructionBegin((ConstructionBeginEvent)buildEvent);
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

        private void ConstructionBegin(ConstructionBeginEvent constructionBegin)
        {
            NitroxServiceLocator.LocateService<Building>().Constructable_ConstructionBegin_Remote(constructionBegin.BasePiece);
        }

        private void ConstructionCompleted(ConstructionCompletedEvent constructionCompleted)
        {
            NitroxServiceLocator.LocateService<Building>().Constructable_ConstructionCompleted_Remote(constructionCompleted.PieceId);
        }

        private void ConstructionAmountChanged(ConstructionAmountChangedEvent amountChanged)
        {
            NitroxServiceLocator.LocateService<Building>().Constructable_AmountChanged_Remote(amountChanged.Id, amountChanged.Amount);
        }

        private void DeconstructionBegin(DeconstructionBeginEvent deconstructBegin)
        {
            NitroxServiceLocator.LocateService<Building>().Constructable_DeconstructionBegin_Remote(deconstructBegin.PieceId);
        }

        private void DeconstructionCompleted(DeconstructionCompletedEvent deconstructCompleted)
        {
            NitroxServiceLocator.LocateService<Building>().Constructable_DeconstructionComplete_Remote(deconstructCompleted.PieceId);
        }
    }
}

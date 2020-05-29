using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxModel.Core;
using NitroxModel.Logger;
using System;
using UnityEngine;
using NitroxModel.GameLogic;


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
        public int Count = 0;
        public WaitScreen.ManualWaitItem WaitItem = null;

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

                if (WaitItem != null && Count != 0)
                {
                    int prog = Count - buildEvents.Count;
                    if (prog < 0)
                    {
                        prog = Convert.ToInt32(Count / 2 - prog * -1 / 2);
                    }
                    else
                    {
                        prog = Convert.ToInt32(prog / 2 + Count / 2);
                    }

                    WaitItem.SetProgress(prog, Count);
                }
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
            NitroxServiceLocator.LocateService<IBuilding>().ConstructNewBasePiece(constructionBegin.BasePiece);
        }

        private void ConstructionCompleted(ConstructionCompletedEvent constructionCompleted)
        {
            NitroxServiceLocator.LocateService<IBuilding>().FinishConstruction(constructionCompleted.PieceId);
        }

        private void ConstructionAmountChanged(ConstructionAmountChangedEvent amountChanged)
        {
            NitroxServiceLocator.LocateService<IBuilding>().ChangeConstructAmount(amountChanged.Id, amountChanged.Amount);
        }

        private void DeconstructionBegin(DeconstructionBeginEvent deconstructBegin)
        {
            NitroxServiceLocator.LocateService<IBuilding>().DeconstructBasePiece(deconstructBegin.PieceId);
        }

        private void DeconstructionCompleted(DeconstructionCompletedEvent deconstructCompleted)
        {
            NitroxServiceLocator.LocateService<IBuilding>().FinishDeconstruction(deconstructCompleted.PieceId);
        }
    }
}

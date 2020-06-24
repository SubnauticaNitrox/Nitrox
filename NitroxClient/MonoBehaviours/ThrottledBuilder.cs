using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic;
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
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.MonoBehaviours
{

    public class ThrottledBuilderProgressEventArgs : EventArgs
    {
        public int ItemsToProcessCount;
        public ThrottledBuilderProgressEventArgs(int itemsToProcessCount)
        {
            ItemsToProcessCount = itemsToProcessCount;
        }
    }


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
        public event EventHandler<ThrottledBuilderProgressEventArgs> ProgressChanged;
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

                if (ProgressChanged != null)
                {
                    ProgressChanged(this, new ThrottledBuilderProgressEventArgs(buildEvents.Count));
                }
            }
        }

        private void ActionBuildEvent(BuildEvent buildEvent)
        {
            if (buildEvent is BasePiecePlacedEvent)
            {
                NitroxServiceLocator.LocateService<Building>().ConstructNewBasePiece(((BasePiecePlacedEvent)buildEvent).BasePiece);
            }
            else if (buildEvent is ConstructionCompletedEvent)
            {
                NitroxServiceLocator.LocateService<Building>().FinishConstruction(((ConstructionCompletedEvent)buildEvent).PieceId);
            }
            else if (buildEvent is ConstructionAmountChangedEvent)
            {
                NitroxServiceLocator.LocateService<Building>().ChangeConstructAmount(((ConstructionAmountChangedEvent)buildEvent).Id, ((ConstructionAmountChangedEvent)buildEvent).Amount);
            }
            else if (buildEvent is DeconstructionBeginEvent)
            {
                NitroxServiceLocator.LocateService<Building>().DeconstructBasePiece(((DeconstructionBeginEvent)buildEvent).PieceId);
            }
            else if (buildEvent is DeconstructionCompletedEvent)
            {
                NitroxServiceLocator.LocateService<Building>().FinishDeconstruction(((DeconstructionCompletedEvent)buildEvent).PieceId);
            }
        }
    }
}

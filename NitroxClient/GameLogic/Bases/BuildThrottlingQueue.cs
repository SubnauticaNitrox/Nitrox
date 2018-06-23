using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using System.Collections.Generic;

namespace NitroxClient.GameLogic.Bases
{
    /**
     * Build events normally can not happen within the same frame as they can cause
     * changes to the surrounding environment.  This class encapsulates logic to 
     * hold build events for future processing.  Incoming packets can be converted
     * to more generic BuildEvent classes that can be re-used. Examples: we want
     * to re-use logic for ConstructionCompleted packet and InitialPlayerSync build
     * packets - this class helps faciliate that.
     */
    public class BuildThrottlingQueue
    {
        private readonly Queue<BuildEvent> pendingBuildEvents = new Queue<BuildEvent>();

        public Optional<BuildEvent> GetNextPendingEvent()
        {
            if (pendingBuildEvents.Count > 0)
            {
                return Optional<BuildEvent>.Of(pendingBuildEvents.Dequeue());
            }

            return Optional<BuildEvent>.Empty();
        }

        public void EnqueueBasePiecePlaced(BasePiece basePiece)
        {
            Log.Info("Enqueuing base piece to be placed " + basePiece.Guid);
            pendingBuildEvents.Enqueue(new BasePiecePlacedEvent(basePiece));
        }

        public void EnqueueConstructionCompleted(string guid, Optional<string> newBaseCreatedGuid)
        {
            Log.Info("Enqueuing item to have construction completed " + guid);
            pendingBuildEvents.Enqueue(new ConstructionCompletedEvent(guid, newBaseCreatedGuid));
        }

        public void EnqueueAmountChanged(string guid, float amount)
        {
            Log.Info("Enqueuing item to have construction amount changed " + guid);
            pendingBuildEvents.Enqueue(new ConstructionAmountChangedEvent(guid, amount));
        }

        public void EnqueueDeconstructionBegin(string guid)
        {
            Log.Info("Enqueuing item to have deconstruction beginning " + guid);
            pendingBuildEvents.Enqueue(new DeconstructionBeginEvent(guid));
        }

        public void EnqueueDeconstructionCompleted(string guid)
        {
            Log.Info("Enqueuing item to have deconstruction completed " + guid);
            pendingBuildEvents.Enqueue(new DeconstructionCompletedEvent(guid));
        }
    }

    public class BasePiecePlacedEvent : BuildEvent
    {
        public BasePiece BasePiece { get; }

        public BasePiecePlacedEvent(BasePiece basePiece)
        {
            BasePiece = basePiece;
        }
    }

    public class ConstructionAmountChangedEvent : BuildEvent
    {
        public string Guid { get; }
        public float Amount { get; }

        public ConstructionAmountChangedEvent(string guid, float amount)
        {
            Guid = guid;
            Amount = amount;
        }
    }

    public class ConstructionCompletedEvent : BuildEvent
    {
        public string Guid { get; }
        public Optional<string> NewBaseCreatedGuid { get; }

        public ConstructionCompletedEvent(string guid, Optional<string> newBaseCreatedGuid)
        {
            Guid = guid;
            NewBaseCreatedGuid = newBaseCreatedGuid;
        }
    }

    public class DeconstructionBeginEvent : BuildEvent
    {
        public string Guid { get; }

        public DeconstructionBeginEvent(string guid)
        {
            Guid = guid;
        }
    }

    public class DeconstructionCompletedEvent : BuildEvent
    {
        public string Guid { get; }

        public DeconstructionCompletedEvent(string guid)
        {
            Guid = guid;
        }
    }

    public interface BuildEvent
    {

    }
}

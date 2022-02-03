using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxClient.GameLogic.Bases
{
    /**
     * Build events normally can not happen within the same frame as they can cause
     * changes to the surrounding environment.  This class encapsulates logic to 
     * hold build events for future processing.  Incoming packets can be converted
     * to more generic BuildEvent classes that can be re-used. Examples: we want
     * to re-use logic for ConstructionCompleted packet and InitialPlayerSync build
     * packets - this class helps facilitate that.
     */
    public class BuildThrottlingQueue : Queue<BuildEvent>
    {
        public bool NextEventRequiresFreshFrame()
        {
            if (Count > 0)
            {
                BuildEvent nextEvent = Peek();
                return nextEvent.RequiresFreshFrame();
            }

            return false;
        }

        public void EnqueueBasePiecePlaced(BasePiece basePiece)
        {
            Log.Info($"Enqueuing base piece to be placed - TechType: {basePiece.TechType}, Id: {basePiece.Id}, ParentId: {basePiece.ParentId.OrElse(null)}, BuildIndex: {basePiece.BuildIndex}");
            Enqueue(new BasePiecePlacedEvent(basePiece));
        }

        public void EnqueueConstructionCompleted(NitroxId id, NitroxId baseId)
        {
            Log.Info($"Enqueuing item to have construction completed - Id: {id}");
            Enqueue(new ConstructionCompletedEvent(id, baseId));
        }

        public void EnqueueLaterConstructionCompleted(NitroxId id, NitroxId baseId)
        {
            Log.Info($"Enqueuing item to have a later construction completed - Id: {id}");
            Enqueue(new LaterConstructionCompletedEvent(id, baseId));
        }

        public void EnqueueAmountChanged(NitroxId id, float amount)
        {
            Log.Info($"Enqueuing item to have construction amount changed - Id: {id}");
            Enqueue(new ConstructionAmountChangedEvent(id, amount));
        }

        public void EnqueueDeconstructionBegin(NitroxId id)
        {
            Log.Info($"Enqueuing item to have deconstruction beginning - Id: {id}");
            Enqueue(new DeconstructionBeginEvent(id));
        }

        public void EnqueueDeconstructionCompleted(NitroxId id)
        {
            Log.Info($"Enqueuing item to have deconstruction completed - Id: {id}");
            Enqueue(new DeconstructionCompletedEvent(id));
        }
    }

    public class BasePiecePlacedEvent : BuildEvent
    {
        public BasePiece BasePiece { get; }

        public BasePiecePlacedEvent(BasePiece basePiece)
        {
            BasePiece = basePiece;
        }

        public bool RequiresFreshFrame()
        {
            // Since furniture can not be built upon, we only require
            // a fresh frame for actual base pieces.
            return !BasePiece.IsFurniture;
        }
    }

    public class ConstructionAmountChangedEvent : BuildEvent
    {
        public NitroxId Id { get; }
        public float Amount { get; }

        public ConstructionAmountChangedEvent(NitroxId id, float amount)
        {
            Id = id;
            Amount = amount;
        }

        public bool RequiresFreshFrame()
        {
            // Change events only affect the materials used and
            // translusence of an item.  We can process multiple
            // of these per a frame.
            return false;
        }
    }

    public class ConstructionCompletedEvent : BuildEvent
    {
        public NitroxId PieceId { get; }
        public NitroxId BaseId { get; }

        public ConstructionCompletedEvent(NitroxId pieceId, NitroxId baseId)
        {
            PieceId = pieceId;
            BaseId = baseId;
        }

        public bool RequiresFreshFrame()
        {
            // Completing construction changes the surrounding
            // environment... We only want to process one per frame.
            return true;
        }
    }

    public class LaterConstructionCompletedEvent : BuildEvent
    {
        public NitroxId PieceId { get; }
        public NitroxId BaseId { get; }

        public LaterConstructionCompletedEvent(NitroxId pieceId, NitroxId baseId)
        {
            PieceId = pieceId;
            BaseId = baseId;
        }

        public bool RequiresFreshFrame()
        {
            // The purpose of this event is to wait for some internal actions to happen to mark the build as complete
            return true;
        }
    }

    public class DeconstructionBeginEvent : BuildEvent
    {
        public NitroxId PieceId { get; }

        public DeconstructionBeginEvent(NitroxId pieceId)
        {
            PieceId = pieceId;
        }

        public bool RequiresFreshFrame()
        {
            // Starting a deconstruction event makes it so you can
            // no longer attach items and may change the surrounding 
            // environment.  Thus, we want to only process one per frame.
            return true;
        }
    }

    public class DeconstructionCompletedEvent : BuildEvent
    {
        public NitroxId PieceId { get; }

        public DeconstructionCompletedEvent(NitroxId pieceId)
        {
            PieceId = pieceId;
        }

        public bool RequiresFreshFrame()
        {
            // Completing a deconstruction will change the surrounding 
            // environment.  Thus, we want to only process one per frame.
            return true;
        }
    }

    public interface BuildEvent
    {
        // Some build events should be processed exclusively in a single
        // frame.  This is usually the case when processing multiple events
        // would cause undeterminitic side-effects.  An example is creating
        // a new base piece will change the environment which needs at least
        // one frame to successfully process.
        bool RequiresFreshFrame();
    }
}

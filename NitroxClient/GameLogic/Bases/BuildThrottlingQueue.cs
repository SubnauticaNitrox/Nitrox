using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using System.Collections.Generic;
using NitroxModel.DataStructures;

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
    public class BuildThrottlingQueue : Queue<BuildEvent>
    {
        public bool NextEventRequiresFreshFrame()
        {
            if(Count > 0)
            {
                BuildEvent nextEvent = Peek();
                return nextEvent.RequiresFreshFrame();
            }

            return false;
        }

        public void EnqueueConstructionBegin(BasePiece basePiece)
        {
            Enqueue(new ConstructionBeginEvent(basePiece));
        }

        public void EnqueueConstructionCompleted(NitroxId id)
        {
            Enqueue(new ConstructionCompletedEvent(id));
        }

        public void EnqueueAmountChanged(NitroxId id, float amount)
        {
            Enqueue(new ConstructionAmountChangedEvent(id, amount));
        }

        public void EnqueueDeconstructionBegin(NitroxId id)
        {
            Enqueue(new DeconstructionBeginEvent(id));
        }

        public void EnqueueDeconstructionCompleted(NitroxId id)
        {
            Enqueue(new DeconstructionCompletedEvent(id));
        }
    }

    public class ConstructionBeginEvent : BuildEvent
    {
        public BasePiece BasePiece { get; }

        public ConstructionBeginEvent(BasePiece basePiece)
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

        public ConstructionCompletedEvent(NitroxId pieceId)
        {
            PieceId = pieceId;
        }

        public bool RequiresFreshFrame()
        {
            // Completing construction changes the surrounding
            // environment... We only want to process one per frame.
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

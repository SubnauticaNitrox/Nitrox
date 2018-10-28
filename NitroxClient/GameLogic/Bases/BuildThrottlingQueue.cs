﻿using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;

namespace NitroxClient.GameLogic.Bases
{
    /// <summary>
    ///     Build events normally can not happen within the same frame as they can cause
    ///     changes to the surrounding environment.  This class encapsulates logic to
    ///     hold build events for future processing.  Incoming packets can be converted
    ///     to more generic BuildEvent classes that can be re-used. Examples: we want
    ///     to re-use logic for ConstructionCompleted packet and InitialPlayerSync build
    ///     packets - this class helps facilitate that.
    /// </summary>
    public class BuildThrottlingQueue : Queue<BuildEvent>
    {
        private readonly INitroxLogger log;

        public BuildThrottlingQueue(INitroxLogger logger)
        {
            log = logger;
        }

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
            log.Info("Enqueuing base piece to be placed " + basePiece.Guid + " " + basePiece.Guid);
            Enqueue(new BasePiecePlacedEvent(basePiece));
        }

        public void EnqueueConstructionCompleted(string guid, Optional<string> newBaseCreatedGuid)
        {
            log.Info("Enqueuing item to have construction completed " + guid);
            Enqueue(new ConstructionCompletedEvent(guid, newBaseCreatedGuid));
        }

        public void EnqueueAmountChanged(string guid, float amount)
        {
            log.Info("Enqueuing item to have construction amount changed " + guid);
            Enqueue(new ConstructionAmountChangedEvent(guid, amount));
        }

        public void EnqueueDeconstructionBegin(string guid)
        {
            log.Info("Enqueuing item to have deconstruction beginning " + guid);
            Enqueue(new DeconstructionBeginEvent(guid));
        }

        public void EnqueueDeconstructionCompleted(string guid)
        {
            log.Info("Enqueuing item to have deconstruction completed " + guid);
            Enqueue(new DeconstructionCompletedEvent(guid));
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
        public string Guid { get; }
        public float Amount { get; }

        public ConstructionAmountChangedEvent(string guid, float amount)
        {
            Guid = guid;
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
        public string Guid { get; }
        public Optional<string> NewBaseCreatedGuid { get; }

        public ConstructionCompletedEvent(string guid, Optional<string> newBaseCreatedGuid)
        {
            Guid = guid;
            NewBaseCreatedGuid = newBaseCreatedGuid;
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
        public string Guid { get; }

        public DeconstructionBeginEvent(string guid)
        {
            Guid = guid;
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
        public string Guid { get; }

        public DeconstructionCompletedEvent(string guid)
        {
            Guid = guid;
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

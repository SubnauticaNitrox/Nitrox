using NitroxModel.DataStructures;

namespace NitroxClient.GameLogic.Simulation
{
    public class LockRequest<T> : LockRequestBase where T : LockRequestContext
    {
        public delegate void LockRequestCompleted(NitroxId id, bool lockAquired, T context);

        private LockRequestCompleted onComplete;
        private T context { get; }

        public LockRequest(NitroxId id, SimulationLockType lockType, LockRequestCompleted onComplete, T context) : base(id, lockType)
        {
            this.onComplete = onComplete;
            this.context = context;
        }

        public override void LockRequestComplete(NitroxId id, bool lockAquired)
        {
            if (onComplete != null)
            {
                onComplete(id, lockAquired, (T)context);
            }
        }

    }
}

using System;

namespace NitroxClient.GameLogic.InitialSync.Base
{
    public abstract class AsyncInitialSyncProcessor : InitialSyncProcessor
    {
        public event EventHandler Completed;
        
        protected void MarkCompleted()
        {
            Completed(this, new EventArgs());
        }
    }
}

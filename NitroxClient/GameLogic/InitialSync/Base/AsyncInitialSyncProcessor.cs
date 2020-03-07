using System;

namespace NitroxClient.GameLogic.InitialSync.Base
{
    public abstract class AsyncInitialSyncProcessor : InitialSyncProcessor
    {
        public event EventHandler Completed;

        // protection from async items throwing their completed event more than once.
        private bool completed = false;

        protected void MarkCompleted()
        {
            if(!completed)
            {
                completed = true;
                Completed(this, new EventArgs());
            }
        }
    }
}

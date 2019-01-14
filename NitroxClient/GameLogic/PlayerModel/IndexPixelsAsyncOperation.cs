using System;
using System.Collections.Generic;
using System.Threading;

namespace NitroxClient.GameLogic.PlayerModel
{
    public class IndexPixelsAsyncOperation
    {
        private readonly Dictionary<string, List<int>> texturePixelIndexes;
        private int runningTaskCount;

        public IndexPixelsAsyncOperation(Dictionary<string, List<int>> texturePixelIndexes)
        {
            this.texturePixelIndexes = texturePixelIndexes;
        }

        public void UpdateIndex(string indexKey, List<int> replacedPixelIndexes)
        {
            lock (texturePixelIndexes)
            {
                if (texturePixelIndexes.ContainsKey(indexKey))
                {
                    throw new ArgumentException($"Texture key {indexKey} already exists.");
                }

                texturePixelIndexes.Add(indexKey, replacedPixelIndexes);
            }

            Interlocked.Decrement(ref runningTaskCount);
        }

        public bool IsComplete()
        {
            return runningTaskCount == 0;
        }

        public IndexPixelsAsyncOperation Start(params Action<IndexPixelsAsyncOperation>[] tasks)
        {
            if (runningTaskCount > 0)
            {
                throw new InvalidOperationException("An indexing operation is already in progress.");
            }

            runningTaskCount = tasks.Length;
            tasks.ForEach(task => ThreadPool.QueueUserWorkItem(ExecuteTask, task));
            

            return this;
        }

        private void ExecuteTask(object state)
        {
            Action<IndexPixelsAsyncOperation> task = state as Action<IndexPixelsAsyncOperation>;

            if(task == null)
            {
                //TODO: We need to handle job cancellation during stabilization to ensure that the client shuts down gracefully.

                throw new ArgumentException(@"Cannot execute a null task.", nameof(task));
            }

            task.Invoke(this);
        }
    }
}

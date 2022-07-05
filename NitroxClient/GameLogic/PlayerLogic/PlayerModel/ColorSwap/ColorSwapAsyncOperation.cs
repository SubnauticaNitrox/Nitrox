using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap
{
    public class ColorSwapAsyncOperation
    {
        private readonly INitroxPlayer nitroxPlayer;
        private readonly IEnumerable<IColorSwapManager> colorSwapManagers;
        private readonly Dictionary<string, Color[]> texturePixelIndexes;
        private int taskCount = -1;

        public ColorSwapAsyncOperation(INitroxPlayer nitroxPlayer, IEnumerable<IColorSwapManager> colorSwapManagers)
        {
            this.nitroxPlayer = nitroxPlayer;
            this.colorSwapManagers = colorSwapManagers;

            texturePixelIndexes = new Dictionary<string, Color[]>();
        }

        public void UpdateIndex(string indexKey, Color[] pixels)
        {
            lock (texturePixelIndexes)
            {
                if (texturePixelIndexes.ContainsKey(indexKey))
                {
                    throw new ArgumentException($"Texture index key {indexKey} already exists.");
                }

                texturePixelIndexes.Add(indexKey, pixels);
            }
        }

        public bool IsColorSwapComplete()
        {
            return taskCount == 0;
        }

        public ColorSwapAsyncOperation BeginColorSwap()
        {
            if (taskCount >= 0)
            {
                throw new InvalidOperationException("This operation has already been started.");
            }

            List<Action<ColorSwapAsyncOperation>> tasks = colorSwapManagers
                .Select(configuration => configuration.CreateColorSwapTask(nitroxPlayer))
                .ToList();

            taskCount = tasks.Count;
            tasks.ForEach(task => ThreadPool.QueueUserWorkItem(ExecuteTask, task));

            return this;
        }

        public void ApplySwappedColors()
        {
            if (taskCount != 0)
            {
                throw new InvalidOperationException("Colors must be swapped before the changes can be applied to the player model.");
            }

            colorSwapManagers.ForEach(manager => manager.ApplyPlayerColor(texturePixelIndexes, nitroxPlayer));
        }

        private void ExecuteTask(object state)
        {
            Action<ColorSwapAsyncOperation> task = state as Action<ColorSwapAsyncOperation>;

            if (task == null)
            {
                //TODO: We need to handle job cancellation during stabilization to ensure that the client shuts down gracefully.

                throw new ArgumentException(@"Cannot execute a null task.", nameof(task));
            }

            task.Invoke(this);

            Interlocked.Decrement(ref taskCount);
        }
    }
}

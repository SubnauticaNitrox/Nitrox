using System;
using System.Collections;
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
        private volatile bool tasksCreated;

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
            return tasksCreated && taskCount == 0;
        }

        public ColorSwapAsyncOperation BeginColorSwap()
        {
            if (taskCount >= 0)
            {
                Log.Error("This operation has already been started.");
                return this;
            }

            List<Action<ColorSwapAsyncOperation>> tasks = colorSwapManagers
                .Select(configuration => configuration.CreateColorSwapTask(nitroxPlayer))
                .ToList();

            taskCount = tasks.Count;
            tasksCreated = true;
            tasks.ForEach(task => ThreadPool.QueueUserWorkItem(ExecuteTask, task));

            return this;
        }

        /// <summary>
        /// Same as <see cref="BeginColorSwap"/>, but creates one manager's task per yielded frame instead of all of
        /// them in one frame. Each <see cref="IColorSwapManager.CreateColorSwapTask"/> call can involve an expensive,
        /// main-thread-only GPU texture readback (see <see cref="Extensions.RendererExtensions.GetSourcePixels(Texture2D)"/>),
        /// so spreading the creation over several frames turns a single noticeable hitch into several smaller, imperceptible ones.
        /// </summary>
        public IEnumerator BeginColorSwapOverFrames()
        {
            if (taskCount >= 0)
            {
                Log.Error("This operation has already been started.");
                yield break;
            }

            IEnumerable<Texture2D> sourceTextures = colorSwapManagers.SelectMany(manager => manager.GetSourceTextures(nitroxPlayer));
            yield return sourceTextures.PrewarmSourcePixelsAsync();

            taskCount = 0;
            foreach (IColorSwapManager manager in colorSwapManagers)
            {
                Action<ColorSwapAsyncOperation> task = manager.CreateColorSwapTask(nitroxPlayer);
                Interlocked.Increment(ref taskCount);
                ThreadPool.QueueUserWorkItem(ExecuteTask, task);
                yield return null;
            }

            tasksCreated = true;
        }

        public void ApplySwappedColors()
        {
            if (taskCount != 0)
            {
                throw new InvalidOperationException("Colors must be swapped before the changes can be applied to the player model.");
            }

            colorSwapManagers.ForEach(manager => manager.ApplyPlayerColor(texturePixelIndexes, nitroxPlayer));
        }

        /// <summary>
        /// Same as <see cref="ApplySwappedColors"/>, but applies one manager's final textures per yielded frame
        /// instead of all managers (up to ~18 texture uploads) in a single frame.
        /// </summary>
        public IEnumerator ApplySwappedColorsOverFrames()
        {
            if (taskCount != 0)
            {
                Log.Error("Colors must be swapped before the changes can be applied to the player model.");
                yield break;
            }

            foreach (IColorSwapManager manager in colorSwapManagers)
            {
                manager.ApplyPlayerColor(texturePixelIndexes, nitroxPlayer);
                yield return null;
            }
        }

        private void ExecuteTask(object state)
        {
            if (state is not Action<ColorSwapAsyncOperation> task)
            {
                //TODO: We need to handle job cancellation during stabilization to ensure that the client shuts down gracefully.
                throw new ArgumentException("Cannot execute a null task.", nameof(state));
            }

            task.Invoke(this);
            Interlocked.Decrement(ref taskCount);
        }
    }
}

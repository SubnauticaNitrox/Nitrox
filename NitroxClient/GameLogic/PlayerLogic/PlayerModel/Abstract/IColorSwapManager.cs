using System;
using System.Collections.Generic;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract
{
    public interface IColorSwapManager
    {
        Action<ColorSwapAsyncOperation> CreateColorSwapTask(INitroxPlayer nitroxPlayer);
        void ApplyPlayerColor(Dictionary<string, Color[]> pixelIndex, INitroxPlayer nitroxPlayer);

        /// <summary>
        /// Every source texture this manager will read pixels from for <paramref name="nitroxPlayer"/>. Used to
        /// kick off an <see cref="UnityEngine.Rendering.AsyncGPUReadback"/> pre-warm for all not-yet-cached
        /// textures up front, instead of stalling the main thread with a synchronous readback the first time each
        /// texture is touched.
        /// </summary>
        IEnumerable<Texture2D> GetSourceTextures(INitroxPlayer nitroxPlayer);
    }
}

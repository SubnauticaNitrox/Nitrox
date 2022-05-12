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
    }
}

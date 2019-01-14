using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModel.Abstract
{
    public interface IColorSwapManager
    {
        Action<ColorSwapAsyncOperation> CreateColorSwapTask(INitroxPlayer nitroxPlayer);
        void ApplyPlayerColor(Dictionary<string, Color[]> pixelIndex, INitroxPlayer nitroxPlayer);
    }
}

﻿using System;
using System.Collections.Generic;
using NitroxClient.GameLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.PlayerModel.ColorSwap.Strategy;
using NitroxClient.Unity.Helper;
using UnityEngine;
using static NitroxClient.GameLogic.PlayerModel.PlayerEquipmentConstants;

namespace NitroxClient.GameLogic.PlayerModel.ColorSwap
{
    public class ScubaTankColorSwapManager : IColorSwapManager
    {
        public Action<ColorSwapAsyncOperation> CreateColorSwapTask(INitroxPlayer nitroxPlayer)
        {
            GameObject playerModel = nitroxPlayer.PlayerModel;
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor;
            IColorSwapStrategy colorSwapStrategy = new HueSwapper(playerColor);

            SkinnedMeshRenderer scubaTankRenderer = playerModel.GetRenderer(SCUBA_TANK_GAME_OBJECT_NAME);
            scubaTankRenderer.material.ApplyClonedTexture();

            Color[] texturePixels = scubaTankRenderer.material.GetMainTexturePixels();

            return operation =>
            {
                HsvSwapper scubaTankFilter = new HsvSwapper(colorSwapStrategy);
                scubaTankFilter.SetHueRange(0f, 30f);

                scubaTankFilter.SwapColors(texturePixels);

                operation.UpdateIndex(SCUBA_TANK_INDEX_KEY, texturePixels);
            };
        }

        public void ApplyPlayerColor(Dictionary<string, Color[]> pixelIndex, INitroxPlayer nitroxPlayer)
        {
            Color[] scubaTankPixelIndexes = pixelIndex[SCUBA_TANK_INDEX_KEY];

            GameObject playerModel = nitroxPlayer.PlayerModel;
            SkinnedMeshRenderer scubaTankRenderer = playerModel.GetRenderer(SCUBA_TANK_GAME_OBJECT_NAME);

            scubaTankRenderer.material.UpdateMainTextureColors(scubaTankPixelIndexes);
            scubaTankRenderer.material.SetTexture("_MainTex", scubaTankRenderer.material.mainTexture);
            scubaTankRenderer.material.SetTexture("_SpecTex", scubaTankRenderer.material.mainTexture);
        }
    }
}

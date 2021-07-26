﻿using System;
using System.Collections.Generic;
using NitroxClient.GameLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.PlayerModel.ColorSwap.Strategy;
using NitroxClient.Unity.Helper;
using UnityEngine;
using static NitroxClient.GameLogic.PlayerModel.PlayerEquipmentConstants;

namespace NitroxClient.GameLogic.PlayerModel.ColorSwap
{
    public class RadiationTankColorSwapManager : IColorSwapManager
    {
        public Action<ColorSwapAsyncOperation> CreateColorSwapTask(INitroxPlayer nitroxPlayer)
        {
            GameObject playerModel = nitroxPlayer.PlayerModel;
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor;
            IColorSwapStrategy colorSwapStrategy = new HueSwapper(playerColor);

            SkinnedMeshRenderer radiationTankRenderer = playerModel.GetRenderer(RADIATION_TANK_GAME_OBJECT_NAME);
            radiationTankRenderer.material.ApplyClonedTexture();

            Color[] texturePixels = radiationTankRenderer.material.GetMainTexturePixels();

            return operation =>
            {
                HsvSwapper radiationTankFilter = new HsvSwapper(colorSwapStrategy);
                radiationTankFilter.SetHueRange(0f, 85f);

                radiationTankFilter.SwapColors(texturePixels);

                operation.UpdateIndex(RADIATION_SUIT_TANK_INDEX_KEY, texturePixels);
            };
        }

        public void ApplyPlayerColor(Dictionary<string, Color[]> pixelIndex, INitroxPlayer nitroxPlayer)
        {
            Color[] tankPixels = pixelIndex[RADIATION_SUIT_TANK_INDEX_KEY];

            GameObject playerModel = nitroxPlayer.PlayerModel;
            SkinnedMeshRenderer radiationTankRenderer = playerModel.GetRenderer(RADIATION_TANK_GAME_OBJECT_NAME);

            radiationTankRenderer.material.UpdateMainTextureColors(tankPixels);
            radiationTankRenderer.material.SetTexture("_MainTex", radiationTankRenderer.material.mainTexture);
            radiationTankRenderer.material.SetTexture("_SpecTex", radiationTankRenderer.material.mainTexture);
        }
    }
}

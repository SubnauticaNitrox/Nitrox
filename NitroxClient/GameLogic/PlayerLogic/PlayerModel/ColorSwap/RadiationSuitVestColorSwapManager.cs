using System;
using System.Collections.Generic;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap.Strategy;
using NitroxClient.Unity.Helper;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using static NitroxClient.GameLogic.PlayerLogic.PlayerModel.PlayerEquipmentConstants;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap
{
    public class RadiationSuitVestColorSwapManager : IColorSwapManager
    {
        public Action<ColorSwapAsyncOperation> CreateColorSwapTask(INitroxPlayer nitroxPlayer)
        {
            GameObject playerModel = nitroxPlayer.PlayerModel;
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor.ToUnity();
            IColorSwapStrategy colorSwapStrategy = new HueSaturationVibrancySwapper(playerColor);

            SkinnedMeshRenderer radiationVestRenderer = playerModel.GetRenderer(RADIATION_SUIT_VEST_GAME_OBJECT_NAME);
            radiationVestRenderer.material.ApplyClonedTexture();

            Color[] texturePixels = radiationVestRenderer.material.GetMainTexturePixels();

            return operation =>
            {
                HsvSwapper radiationSuitVestFilter = new HsvSwapper(colorSwapStrategy);
                radiationSuitVestFilter.SetSaturationRange(0f, 35f);
                radiationSuitVestFilter.SetVibrancyRange(12f, 100f);

                radiationSuitVestFilter.SwapColors(texturePixels);

                operation.UpdateIndex(RADIATION_SUIT_VEST_INDEX_KEY, texturePixels);
            };
        }

        public void ApplyPlayerColor(Dictionary<string, Color[]> pixelIndex, INitroxPlayer nitroxPlayer)
        {
            Color[] helmetPixels = pixelIndex[RADIATION_SUIT_VEST_INDEX_KEY];

            GameObject playerModel = nitroxPlayer.PlayerModel;
            SkinnedMeshRenderer radiationHelmetRenderer = playerModel.GetRenderer(RADIATION_SUIT_VEST_GAME_OBJECT_NAME);
            radiationHelmetRenderer.material.UpdateMainTextureColors(helmetPixels);
        }
    }
}

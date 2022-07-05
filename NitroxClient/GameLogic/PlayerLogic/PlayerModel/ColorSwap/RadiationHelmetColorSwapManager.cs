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
    public class RadiationHelmetColorSwapManager : IColorSwapManager
    {
        public Action<ColorSwapAsyncOperation> CreateColorSwapTask(INitroxPlayer nitroxPlayer)
        {
            GameObject playerModel = nitroxPlayer.PlayerModel;
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor.ToUnity();
            IColorSwapStrategy colorSwapStrategy = new HueSaturationVibrancySwapper(playerColor);

            SkinnedMeshRenderer radiationHelmetRenderer = playerModel.GetRenderer(RADIATION_HELMET_GAME_OBJECT_NAME);
            radiationHelmetRenderer.material.ApplyClonedTexture();

            SkinnedMeshRenderer radiationSuitNeckClaspRenderer = playerModel.GetRenderer(RADIATION_SUIT_NECK_CLASP_GAME_OBJECT_NAME);
            radiationSuitNeckClaspRenderer.material.ApplyClonedTexture();

            Color[] helmetPixels = radiationHelmetRenderer.material.GetMainTexturePixels();
            Color[] neckClaspPixels = radiationSuitNeckClaspRenderer.material.GetMainTexturePixels();

            return operation =>
            {
                HsvSwapper radiationHelmetFilter = new HsvSwapper(colorSwapStrategy);
                radiationHelmetFilter.SetSaturationRange(0f, 35f);
                radiationHelmetFilter.SetVibrancyRange(30f, 100f);

                radiationHelmetFilter.SwapColors(helmetPixels);
                radiationHelmetFilter.SwapColors(neckClaspPixels);

                operation.UpdateIndex(RADIATION_HELMET_INDEX_KEY, helmetPixels);
                operation.UpdateIndex(RADIATION_SUIT_NECK_CLASP_INDEX_KEY, helmetPixels);
            };
        }

        public void ApplyPlayerColor(Dictionary<string, Color[]> pixelIndex, INitroxPlayer nitroxPlayer)
        {
            Color[] helmetPixels = pixelIndex[RADIATION_HELMET_INDEX_KEY];
            Color[] neckClaspPixels = pixelIndex[RADIATION_SUIT_NECK_CLASP_INDEX_KEY];

            GameObject playerModel = nitroxPlayer.PlayerModel;

            SkinnedMeshRenderer radiationHelmetRenderer = playerModel.GetRenderer(RADIATION_HELMET_GAME_OBJECT_NAME);
            radiationHelmetRenderer.material.UpdateMainTextureColors(helmetPixels);

            SkinnedMeshRenderer radiationSuitNeckClaspRenderer = playerModel.GetRenderer(RADIATION_SUIT_NECK_CLASP_GAME_OBJECT_NAME);
            radiationSuitNeckClaspRenderer.material.UpdateMainTextureColors(neckClaspPixels);
        }
    }
}

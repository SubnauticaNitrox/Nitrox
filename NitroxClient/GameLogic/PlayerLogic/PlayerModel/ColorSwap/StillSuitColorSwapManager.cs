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
    public class StillSuitColorSwapManager : IColorSwapManager
    {
        public Action<ColorSwapAsyncOperation> CreateColorSwapTask(INitroxPlayer nitroxPlayer)
        {
            GameObject playerModel = nitroxPlayer.PlayerModel;
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor.ToUnity();
            IColorSwapStrategy colorSwapStrategy = new HueSwapper(playerColor);

            SkinnedMeshRenderer stillSuitRenderer = playerModel.GetRenderer(STILL_SUIT_GAME_OBJECT_NAME);
            stillSuitRenderer.material.ApplyClonedTexture();
            stillSuitRenderer.materials[1].ApplyClonedTexture();

            Color[] bodyTexturePixels = stillSuitRenderer.material.GetMainTexturePixels();
            Color[] armsTexturePixels = stillSuitRenderer.materials[1].GetMainTexturePixels();

            return operation =>
            {
                HsvSwapper stillSuitFilter = new HsvSwapper(colorSwapStrategy);
                stillSuitFilter.SetHueRange(0f, 75f);

                stillSuitFilter.SwapColors(bodyTexturePixels);
                stillSuitFilter.SwapColors(armsTexturePixels);

                operation.UpdateIndex(STILL_SUIT_INDEX_KEY, bodyTexturePixels);
                operation.UpdateIndex(STILL_SUIT_ARMS_INDEX_KEY, armsTexturePixels);
            };
        }

        public void ApplyPlayerColor(Dictionary<string, Color[]> pixelIndex, INitroxPlayer nitroxPlayer)
        {
            Color[] bodyPixelIndexes = pixelIndex[STILL_SUIT_INDEX_KEY];
            Color[] armsPixelIndexes = pixelIndex[STILL_SUIT_ARMS_INDEX_KEY];

            GameObject playerModel = nitroxPlayer.PlayerModel;

            SkinnedMeshRenderer stillSuitRenderer = playerModel.GetRenderer(STILL_SUIT_GAME_OBJECT_NAME);
            stillSuitRenderer.material.UpdateMainTextureColors(bodyPixelIndexes);
            stillSuitRenderer.material.SetTexture("_MainTex", stillSuitRenderer.material.mainTexture);
            stillSuitRenderer.material.SetTexture("_SpecTex", stillSuitRenderer.material.mainTexture);

            stillSuitRenderer.materials[1].UpdateMainTextureColors(armsPixelIndexes);
            stillSuitRenderer.materials[1].SetTexture("_MainTex", stillSuitRenderer.materials[1].mainTexture);
            stillSuitRenderer.materials[1].SetTexture("_SpecTex", stillSuitRenderer.materials[1].mainTexture);
        }
    }
}

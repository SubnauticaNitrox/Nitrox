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
    public class DiveSuitColorSwapManager : IColorSwapManager
    {
        public Action<ColorSwapAsyncOperation> CreateColorSwapTask(INitroxPlayer nitroxPlayer)
        {
            GameObject playerModel = nitroxPlayer.PlayerModel;
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor.ToUnity();
            IColorSwapStrategy colorSwapStrategy = new HueSwapper(playerColor);

            SkinnedMeshRenderer diveSuitRenderer = playerModel.GetRenderer(DIVE_SUIT_GAME_OBJECT_NAME);
            diveSuitRenderer.material.ApplyClonedTexture();
            diveSuitRenderer.materials[1].ApplyClonedTexture();

            Color[] bodyTexturePixels = diveSuitRenderer.material.GetMainTexturePixels();
            Color[] armTexturePixels = diveSuitRenderer.materials[1].GetMainTexturePixels();

            return operation =>
            {
                HsvSwapper diveSuitFilter = new HsvSwapper(colorSwapStrategy);
                diveSuitFilter.SetHueRange(5f, 45f);

                diveSuitFilter.SwapColors(bodyTexturePixels);
                diveSuitFilter.SwapColors(armTexturePixels);

                operation.UpdateIndex(DIVE_SUIT_INDEX_KEY, bodyTexturePixels);
                operation.UpdateIndex(DIVE_SUIT_ARMS_INDEX_KEY, armTexturePixels);
            };
        }

        public void ApplyPlayerColor(Dictionary<string, Color[]> pixelIndex, INitroxPlayer nitroxPlayer)
        {
            Color[] bodyPixels = pixelIndex[DIVE_SUIT_INDEX_KEY];
            Color[] armSleevesPixels = pixelIndex[DIVE_SUIT_ARMS_INDEX_KEY];

            GameObject playerModel = nitroxPlayer.PlayerModel;
            SkinnedMeshRenderer renderer = playerModel.GetRenderer(DIVE_SUIT_GAME_OBJECT_NAME);

            Material torsoMaterial = renderer.material;
            torsoMaterial.UpdateMainTextureColors(bodyPixels);

            Material armsMaterial = renderer.materials[1];
            armsMaterial.UpdateMainTextureColors(armSleevesPixels);
        }
    }
}

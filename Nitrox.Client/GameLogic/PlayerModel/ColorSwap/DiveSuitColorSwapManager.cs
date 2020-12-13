using System;
using System.Collections.Generic;
using Nitrox.Client.GameLogic.PlayerModel.Abstract;
using Nitrox.Client.GameLogic.PlayerModel.ColorSwap.Strategy;
using Nitrox.Client.Unity.Helper;
using Nitrox.Model.Subnautica.DataStructures;
using UnityEngine;
using static Nitrox.Client.GameLogic.PlayerModel.PlayerEquipmentConstants;

namespace Nitrox.Client.GameLogic.PlayerModel.ColorSwap
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

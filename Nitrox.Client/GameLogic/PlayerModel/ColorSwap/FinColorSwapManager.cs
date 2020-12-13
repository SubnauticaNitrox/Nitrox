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
    public class FinColorSwapManager : IColorSwapManager
    {
        public Action<ColorSwapAsyncOperation> CreateColorSwapTask(INitroxPlayer nitroxPlayer)
        {
            GameObject playerModel = nitroxPlayer.PlayerModel;
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor.ToUnity();
            IColorSwapStrategy colorSwapStrategy = new HueSwapper(playerColor);

            SkinnedMeshRenderer basicFinRenderer = playerModel.GetRenderer(FINS_GAME_OBJECT_NAME);
            basicFinRenderer.material.ApplyClonedTexture();

            SkinnedMeshRenderer chargedFinRenderer = playerModel.GetRenderer(CHARGED_FINS_GAME_OBJECT_NAME);
            chargedFinRenderer.material.ApplyClonedTexture();

            SkinnedMeshRenderer glideFinRenderer = playerModel.GetRenderer(GLIDE_FINS_GAME_OBJECT_NAME);
            glideFinRenderer.material.ApplyClonedTexture();

            //All fin models use the same texture.
            Color[] texturePixels = basicFinRenderer.material.GetMainTexturePixels();

            return operation =>
            {
                HsvSwapper finFilter = new HsvSwapper(colorSwapStrategy);
                finFilter.SetHueRange(0f, 35f);

                finFilter.SwapColors(texturePixels);

                operation.UpdateIndex(FINS_INDEX_EY, texturePixels);
            };
        }

        public void ApplyPlayerColor(Dictionary<string, Color[]> pixelIndex, INitroxPlayer nitroxPlayer)
        {
            Color[] pixels = pixelIndex[FINS_INDEX_EY];

            GameObject playerModel = nitroxPlayer.PlayerModel;

            SkinnedMeshRenderer basicFinRenderer = playerModel.GetRenderer(FINS_GAME_OBJECT_NAME);
            basicFinRenderer.material.UpdateMainTextureColors(pixels);
            basicFinRenderer.material.SetTexture("_MainTex", basicFinRenderer.material.mainTexture);
            basicFinRenderer.material.SetTexture("_SpecTex", basicFinRenderer.material.mainTexture);

            SkinnedMeshRenderer chargedFinRenderer = playerModel.GetRenderer(CHARGED_FINS_GAME_OBJECT_NAME);
            chargedFinRenderer.material.UpdateMainTextureColors(pixels);
            chargedFinRenderer.material.SetTexture("_MainTex", chargedFinRenderer.material.mainTexture);
            chargedFinRenderer.material.SetTexture("_SpecTex", chargedFinRenderer.material.mainTexture);

            SkinnedMeshRenderer glideFinRenderer = playerModel.GetRenderer(GLIDE_FINS_GAME_OBJECT_NAME);
            glideFinRenderer.material.UpdateMainTextureColors(pixels);
            glideFinRenderer.material.SetTexture("_MainTex", glideFinRenderer.material.mainTexture);
            glideFinRenderer.material.SetTexture("_SpecTex", glideFinRenderer.material.mainTexture);
        }
    }
}

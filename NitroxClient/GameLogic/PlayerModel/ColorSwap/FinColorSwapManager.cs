using System;
using System.Collections.Generic;
using NitroxClient.GameLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.PlayerModel.ColorSwap.Strategy;
using NitroxClient.Unity.Helper;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using static NitroxClient.GameLogic.PlayerModel.PlayerEquipmentConstants;

namespace NitroxClient.GameLogic.PlayerModel.ColorSwap
{
    public class FinColorSwapManager : IColorSwapManager
    {
        private static readonly int mainTex = Shader.PropertyToID("_MainTex");
        private static readonly int specTex = Shader.PropertyToID("_SpecTex");

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
            basicFinRenderer.material.SetTexture(mainTex, basicFinRenderer.material.mainTexture);
            basicFinRenderer.material.SetTexture(specTex, basicFinRenderer.material.mainTexture);

            SkinnedMeshRenderer chargedFinRenderer = playerModel.GetRenderer(CHARGED_FINS_GAME_OBJECT_NAME);
            chargedFinRenderer.material.UpdateMainTextureColors(pixels);
            chargedFinRenderer.material.SetTexture(mainTex, chargedFinRenderer.material.mainTexture);
            chargedFinRenderer.material.SetTexture(specTex, chargedFinRenderer.material.mainTexture);

            SkinnedMeshRenderer glideFinRenderer = playerModel.GetRenderer(GLIDE_FINS_GAME_OBJECT_NAME);
            glideFinRenderer.material.UpdateMainTextureColors(pixels);
            glideFinRenderer.material.SetTexture(mainTex, glideFinRenderer.material.mainTexture);
            glideFinRenderer.material.SetTexture(specTex, glideFinRenderer.material.mainTexture);
        }
    }
}

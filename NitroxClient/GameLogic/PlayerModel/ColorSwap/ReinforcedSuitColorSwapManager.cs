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
    public class ReinforcedSuitColorSwapManager : IColorSwapManager
    {
        private static readonly int mainTex = Shader.PropertyToID("_MainTex");
        private static readonly int specTex = Shader.PropertyToID("_SpecTex");

        public Action<ColorSwapAsyncOperation> CreateColorSwapTask(INitroxPlayer nitroxPlayer)
        {
            GameObject playerModel = nitroxPlayer.PlayerModel;
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor.ToUnity();
            IColorSwapStrategy colorSwapStrategy = new HueSwapper(playerColor);

            SkinnedMeshRenderer reinforcedSuitRenderer = playerModel.GetRenderer(REINFORCED_SUIT_GAME_OBJECT_NAME);
            reinforcedSuitRenderer.material.ApplyClonedTexture();
            reinforcedSuitRenderer.materials[1].ApplyClonedTexture();

            SkinnedMeshRenderer reinforcedGloveRenderer = playerModel.GetRenderer(REINFORCED_GLOVES_GAME_OBJECT_NAME);
            reinforcedGloveRenderer.material.ApplyClonedTexture();

            Color[] suitTexturePixels = reinforcedSuitRenderer.material.GetMainTexturePixels();
            Color[] armsTexturePixels = reinforcedSuitRenderer.materials[1].GetMainTexturePixels();
            Color[] gloveTexturePixels = reinforcedGloveRenderer.material.GetMainTexturePixels();

            return operation =>
            {
                HsvSwapper reinforcedSuitFilter = new HsvSwapper(colorSwapStrategy);
                reinforcedSuitFilter.SetHueRange(0f, 20f);
                reinforcedSuitFilter.SetSaturationRange(45f, 100f);

                reinforcedSuitFilter.SwapColors(suitTexturePixels);
                reinforcedSuitFilter.SwapColors(armsTexturePixels);
                reinforcedSuitFilter.SwapColors(gloveTexturePixels);

                operation.UpdateIndex(REINFORCED_SUIT_INDEX_KEY, suitTexturePixels);
                operation.UpdateIndex(REINFORCED_SUIT_ARMS_INDEX_KEY, armsTexturePixels);
                operation.UpdateIndex(REINFORCED_GLOVES_INDEX_KEY, gloveTexturePixels);
            };
        }

        public void ApplyPlayerColor(Dictionary<string, Color[]> pixelIndex, INitroxPlayer nitroxPlayer)
        {
            Color[] suitPixelIndexes = pixelIndex[REINFORCED_SUIT_INDEX_KEY];
            Color[] armsTexturePixels = pixelIndex[REINFORCED_SUIT_ARMS_INDEX_KEY];
            Color[] glovePixelIndexes = pixelIndex[REINFORCED_GLOVES_INDEX_KEY];

            GameObject playerModel = nitroxPlayer.PlayerModel;

            SkinnedMeshRenderer reinforcedSuitRenderer = playerModel.GetRenderer(REINFORCED_SUIT_GAME_OBJECT_NAME);
            reinforcedSuitRenderer.material.UpdateMainTextureColors(suitPixelIndexes);
            reinforcedSuitRenderer.material.SetTexture(mainTex, reinforcedSuitRenderer.material.mainTexture);
            reinforcedSuitRenderer.material.SetTexture(specTex, reinforcedSuitRenderer.material.mainTexture);

            reinforcedSuitRenderer.materials[1].UpdateMainTextureColors(armsTexturePixels);
            reinforcedSuitRenderer.materials[1].SetTexture(mainTex, reinforcedSuitRenderer.materials[1].mainTexture);
            reinforcedSuitRenderer.materials[1].SetTexture(specTex, reinforcedSuitRenderer.materials[1].mainTexture);

            SkinnedMeshRenderer reinforcedGlovesRenderer = playerModel.GetRenderer(REINFORCED_GLOVES_GAME_OBJECT_NAME);
            reinforcedGlovesRenderer.material.UpdateMainTextureColors(glovePixelIndexes);
        }
    }
}

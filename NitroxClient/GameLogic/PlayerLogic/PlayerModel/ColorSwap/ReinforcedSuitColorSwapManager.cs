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
    public class ReinforcedSuitColorSwapManager : IColorSwapManager
    {
        public void PrepareMaterials(GameObject playerModel)
        {
            SkinnedMeshRenderer reinforcedSuitRenderer = playerModel.GetRenderer(REINFORCED_SUIT_GAME_OBJECT_NAME);

            SkinnedMeshRenderer reinforcedSuitGlovesRenderer = playerModel.GetRenderer(REINFORCED_GLOVES_GAME_OBJECT_NAME);
        }

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
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor.ToUnity();
            IColorSwapStrategy colorSwapStrategy = new HueSwapper(playerColor);

            SkinnedMeshRenderer reinforcedSuitRenderer = playerModel.GetRenderer(REINFORCED_SUIT_GAME_OBJECT_NAME);
            reinforcedSuitRenderer.material.UpdateMainTextureColors(suitPixelIndexes);
            reinforcedSuitRenderer.material.SetTexture("_MainTex", reinforcedSuitRenderer.material.mainTexture);
            reinforcedSuitRenderer.material.SetTexture("_SpecTex", reinforcedSuitRenderer.material.mainTexture);

            reinforcedSuitRenderer.materials[1].UpdateMainTextureColors(armsTexturePixels);
            reinforcedSuitRenderer.materials[1].SetTexture("_MainTex", reinforcedSuitRenderer.materials[1].mainTexture);
            reinforcedSuitRenderer.materials[1].SetTexture("_SpecTex", reinforcedSuitRenderer.materials[1].mainTexture);

            SkinnedMeshRenderer reinforcedGlovesRenderer = playerModel.GetRenderer(REINFORCED_GLOVES_GAME_OBJECT_NAME);
            reinforcedGlovesRenderer.material.UpdateMainTextureColors(glovePixelIndexes);
        }
    }
}

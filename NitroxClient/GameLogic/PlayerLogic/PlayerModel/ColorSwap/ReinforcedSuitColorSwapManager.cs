using System;
using System.Collections.Generic;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap.Strategy;
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

            SkinnedMeshRenderer reinforcedGloveRenderer = playerModel.GetRenderer(REINFORCED_GLOVES_GAME_OBJECT_NAME);

            Color[] suitTexturePixels = reinforcedSuitRenderer.material.GetSourcePixels();
            Color[] armsTexturePixels = reinforcedSuitRenderer.materials[1].GetSourcePixels();
            Color[] gloveTexturePixels = reinforcedGloveRenderer.material.GetSourcePixels();

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

        public IEnumerable<Texture2D> GetSourceTextures(INitroxPlayer nitroxPlayer)
        {
            SkinnedMeshRenderer reinforcedSuitRenderer = nitroxPlayer.PlayerModel.GetRenderer(REINFORCED_SUIT_GAME_OBJECT_NAME);
            yield return (Texture2D)reinforcedSuitRenderer.material.mainTexture;
            yield return (Texture2D)reinforcedSuitRenderer.materials[1].mainTexture;
            yield return (Texture2D)nitroxPlayer.PlayerModel.GetRenderer(REINFORCED_GLOVES_GAME_OBJECT_NAME).material.mainTexture;
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

#if BELOWZERO
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
    public class ColdProtectiveSuitColorSwapManager : IColorSwapManager
    {
        public Action<ColorSwapAsyncOperation> CreateColorSwapTask(INitroxPlayer nitroxPlayer)
        {
            GameObject playerModel = nitroxPlayer.PlayerModel;
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor.ToUnity();
            IColorSwapStrategy colorSwapStrategy = new HueSwapper(playerColor);

            SkinnedMeshRenderer coldProtectiveHeadRenderer = playerModel.GetRenderer(COLD_PROTECTIVE_HEAD_GAME_OBJECT_NAME);
            coldProtectiveHeadRenderer.material.ApplyClonedTexture();

            SkinnedMeshRenderer coldProtectiveMaskRenderer = playerModel.GetRenderer(COLD_PROTECTIVE_MASK_GAME_OBJECT_NAME);
            coldProtectiveMaskRenderer.material.ApplyClonedTexture();

            SkinnedMeshRenderer coldProtectiveSuitRenderer = playerModel.GetRenderer(COLD_PROTECTIVE_BODY_GAME_OBJECT_NAME);
            coldProtectiveSuitRenderer.material.ApplyClonedTexture();
            coldProtectiveSuitRenderer.materials[1].ApplyClonedTexture();

            SkinnedMeshRenderer coldProtectiveHandsRenderer = playerModel.GetRenderer(COLD_PROTECTIVE_HANDS_GAME_OBJECT_NAME);
            coldProtectiveHandsRenderer.material.ApplyClonedTexture();

            Color[] headTexturePixels = coldProtectiveHeadRenderer.material.GetMainTexturePixels();
            Color[] maskTexturePixels = coldProtectiveMaskRenderer.material.GetMainTexturePixels();
            Color[] suitTexturePixels = coldProtectiveSuitRenderer.material.GetMainTexturePixels();
            Color[] armsTexturePixels = coldProtectiveSuitRenderer.materials[1].GetMainTexturePixels();
            Color[] handsTexturePixels = coldProtectiveHandsRenderer.material.GetMainTexturePixels();

            return operation =>
            {
                HsvSwapper coldProtectiveSuitFilter = new HsvSwapper(colorSwapStrategy);
                coldProtectiveSuitFilter.SetHueRange(0f, 20f);
                coldProtectiveSuitFilter.SetSaturationRange(45f, 100f);
                coldProtectiveSuitFilter.SwapColors(headTexturePixels);
                coldProtectiveSuitFilter.SwapColors(maskTexturePixels);
                coldProtectiveSuitFilter.SwapColors(suitTexturePixels);
                coldProtectiveSuitFilter.SwapColors(armsTexturePixels);
                coldProtectiveSuitFilter.SwapColors(handsTexturePixels);
                operation.UpdateIndex(COLD_PROTECTIVE_HEAD_INDEX_KEY, headTexturePixels);
                operation.UpdateIndex(COLD_PROTECTIVE_MASK_INDEX_KEY, maskTexturePixels);
                operation.UpdateIndex(COLD_PROTECTIVE_BODY_INDEX_KEY, suitTexturePixels);
                operation.UpdateIndex(COLD_PROTECTIVE_ARMS_INDEX_KEY, armsTexturePixels);
                operation.UpdateIndex(COLD_PROTECTIVE_HANDS_INDEX_KEY, handsTexturePixels);
            };
        }

        public void ApplyPlayerColor(Dictionary<string, Color[]> pixelIndex, INitroxPlayer nitroxPlayer)
        {
            Color[] headPixelIndexes = pixelIndex[COLD_PROTECTIVE_HEAD_INDEX_KEY];
            Color[] maskPixelIndexes = pixelIndex[COLD_PROTECTIVE_MASK_INDEX_KEY];
            Color[] suitPixelIndexes = pixelIndex[COLD_PROTECTIVE_BODY_INDEX_KEY];
            Color[] armsTexturePixels = pixelIndex[COLD_PROTECTIVE_ARMS_INDEX_KEY];
            Color[] handsPixelIndexes = pixelIndex[COLD_PROTECTIVE_HANDS_INDEX_KEY];

            GameObject playerModel = nitroxPlayer.PlayerModel;

            SkinnedMeshRenderer coldProtectiveHeadRenderer = playerModel.GetRenderer(COLD_PROTECTIVE_HEAD_GAME_OBJECT_NAME);
            coldProtectiveHeadRenderer.material.UpdateMainTextureColors(headPixelIndexes);

            SkinnedMeshRenderer coldProtectiveMaskRenderer = playerModel.GetRenderer(COLD_PROTECTIVE_MASK_GAME_OBJECT_NAME);
            coldProtectiveMaskRenderer.material.UpdateMainTextureColors(maskPixelIndexes);

            SkinnedMeshRenderer coldProtectiveSuitRenderer = playerModel.GetRenderer(COLD_PROTECTIVE_BODY_GAME_OBJECT_NAME);
            coldProtectiveSuitRenderer.material.UpdateMainTextureColors(suitPixelIndexes);
            coldProtectiveSuitRenderer.material.SetTexture("_MainTex", coldProtectiveSuitRenderer.material.mainTexture);
            coldProtectiveSuitRenderer.material.SetTexture("_SpecTex", coldProtectiveSuitRenderer.material.mainTexture);

            coldProtectiveSuitRenderer.materials[1].UpdateMainTextureColors(armsTexturePixels);
            coldProtectiveSuitRenderer.materials[1].SetTexture("_MainTex", coldProtectiveSuitRenderer.materials[1].mainTexture);
            coldProtectiveSuitRenderer.materials[1].SetTexture("_SpecTex", coldProtectiveSuitRenderer.materials[1].mainTexture);

            SkinnedMeshRenderer reinforcedHandsRenderer = playerModel.GetRenderer(REINFORCED_GLOVES_GAME_OBJECT_NAME);
            reinforcedHandsRenderer.material.UpdateMainTextureColors(handsPixelIndexes);
        }
    }
}
#endif

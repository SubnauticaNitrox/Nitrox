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
    public class RadiationSuitColorSwapManager : IColorSwapManager
    {
        private readonly TextureBlock beltTextureBlock;
        private readonly TextureBlock feetTextureBlock;
        private readonly TextureBlock legTextureBlock;

        public RadiationSuitColorSwapManager()
        {
            legTextureBlock = new TextureBlock(700, 484, 130, 155);
            feetTextureBlock = new TextureBlock(525, 324, 250, 325);
            beltTextureBlock = new TextureBlock(570, 0, 454, 1024);
        }

        public Action<ColorSwapAsyncOperation> CreateColorSwapTask(INitroxPlayer nitroxPlayer)
        {
            GameObject playerModel = nitroxPlayer.PlayerModel;
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor.ToUnity();

            HueSaturationVibrancySwapper hueSaturationVibrancySwapper = new HueSaturationVibrancySwapper(playerColor);
            HueSwapper hueSwapper = new HueSwapper(playerColor);

            SkinnedMeshRenderer radiationSuitRenderer = playerModel.GetRenderer(RADIATION_SUIT_GAME_OBJECT_NAME);
            radiationSuitRenderer.material.ApplyClonedTexture();
            radiationSuitRenderer.materials[1].ApplyClonedTexture();

            Color[] legPixelBlock = radiationSuitRenderer.material.GetMainTexturePixelBlock(legTextureBlock);
            Color[] feetPixelBlock = radiationSuitRenderer.material.GetMainTexturePixelBlock(feetTextureBlock);
            Color[] beltPixelBlock = radiationSuitRenderer.material.GetMainTexturePixelBlock(beltTextureBlock);
            Color[] armSleevesPixels = radiationSuitRenderer.materials[1].GetMainTexturePixels();

            return operation =>
            {
                HsvSwapper radiationSuitLegFilter = new HsvSwapper(hueSaturationVibrancySwapper);
                radiationSuitLegFilter.SetSaturationRange(0f, 35f);
                radiationSuitLegFilter.SetVibrancyRange(40f, 100f);

                HsvSwapper radiationSuitArmAndFeetFilter = new HsvSwapper(hueSwapper);
                radiationSuitArmAndFeetFilter.SetHueRange(0f, 100f);
                radiationSuitArmAndFeetFilter.SetVibrancyRange(30f, 100f);

                HsvSwapper radiationSuitBeltFilter = new HsvSwapper(hueSwapper);
                radiationSuitBeltFilter.SetVibrancyRange(3f, 100f);
                radiationSuitBeltFilter.SetHueRange(0f, 90f);

                radiationSuitLegFilter.SwapColors(legPixelBlock);
                radiationSuitArmAndFeetFilter.SwapColors(feetPixelBlock);
                radiationSuitArmAndFeetFilter.SwapColors(armSleevesPixels);
                radiationSuitBeltFilter.SwapColors(beltPixelBlock);

                operation.UpdateIndex(RADIATION_SUIT_ARMS_INDEX_KEY, armSleevesPixels);
                operation.UpdateIndex(RADIATION_SUIT_LEG_INDEX_KEY, legPixelBlock);
                operation.UpdateIndex(RADIATION_SUIT_FEET_INDEX_KEY, feetPixelBlock);
                operation.UpdateIndex(RADIATION_SUIT_BELT_INDEX_KEY, beltPixelBlock);
            };
        }

        public void ApplyPlayerColor(Dictionary<string, Color[]> pixelIndex, INitroxPlayer nitroxPlayer)
        {
            Color[] armSleevesPixels = pixelIndex[RADIATION_SUIT_ARMS_INDEX_KEY];
            Color[] legPixels = pixelIndex[RADIATION_SUIT_LEG_INDEX_KEY];
            Color[] feetPixels = pixelIndex[RADIATION_SUIT_FEET_INDEX_KEY];
            Color[] beltPixels = pixelIndex[RADIATION_SUIT_BELT_INDEX_KEY];

            GameObject playerModel = nitroxPlayer.PlayerModel;

            SkinnedMeshRenderer radiationSuitRenderer = playerModel.GetRenderer(RADIATION_SUIT_GAME_OBJECT_NAME);

            radiationSuitRenderer.material.UpdateMainTextureColors(legPixels, legTextureBlock);
            radiationSuitRenderer.material.UpdateMainTextureColors(feetPixels, feetTextureBlock);
            radiationSuitRenderer.material.UpdateMainTextureColors(beltPixels, beltTextureBlock);
            radiationSuitRenderer.materials[1].UpdateMainTextureColors(armSleevesPixels);

            radiationSuitRenderer.material.SetTexture("_MainText", radiationSuitRenderer.material.mainTexture);
            radiationSuitRenderer.material.SetTexture("_SpecTex", radiationSuitRenderer.material.mainTexture);
            radiationSuitRenderer.materials[1].SetTexture("_MainText", radiationSuitRenderer.materials[1].mainTexture);
            radiationSuitRenderer.materials[1].SetTexture("_SpecTex", radiationSuitRenderer.materials[1].mainTexture);
        }
    }
}

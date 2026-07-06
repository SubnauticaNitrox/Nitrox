using System;
using System.Collections.Generic;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap.Strategy;
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

            int textureWidth = ((Texture2D)radiationSuitRenderer.material.mainTexture).width;
            Color[] fullPixels = radiationSuitRenderer.material.GetSourcePixels();

            Color[] legPixelBlock = fullPixels.ExtractBlock(textureWidth, legTextureBlock);
            Color[] feetPixelBlock = fullPixels.ExtractBlock(textureWidth, feetTextureBlock);
            Color[] beltPixelBlock = fullPixels.ExtractBlock(textureWidth, beltTextureBlock);
            Color[] armSleevesPixels = radiationSuitRenderer.materials[1].GetSourcePixels();

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

        public IEnumerable<Texture2D> GetSourceTextures(INitroxPlayer nitroxPlayer)
        {
            SkinnedMeshRenderer radiationSuitRenderer = nitroxPlayer.PlayerModel.GetRenderer(RADIATION_SUIT_GAME_OBJECT_NAME);
            yield return (Texture2D)radiationSuitRenderer.material.mainTexture;
            yield return (Texture2D)radiationSuitRenderer.materials[1].mainTexture;
        }

        public void ApplyPlayerColor(Dictionary<string, Color[]> pixelIndex, INitroxPlayer nitroxPlayer)
        {
            Color[] armSleevesPixels = pixelIndex[RADIATION_SUIT_ARMS_INDEX_KEY];
            Color[] legPixels = pixelIndex[RADIATION_SUIT_LEG_INDEX_KEY];
            Color[] feetPixels = pixelIndex[RADIATION_SUIT_FEET_INDEX_KEY];
            Color[] beltPixels = pixelIndex[RADIATION_SUIT_BELT_INDEX_KEY];

            GameObject playerModel = nitroxPlayer.PlayerModel;

            SkinnedMeshRenderer radiationSuitRenderer = playerModel.GetRenderer(RADIATION_SUIT_GAME_OBJECT_NAME);

            // Patch the swapped blocks into a fresh full-texture copy so the whole leg/feet/belt texture can be
            // uploaded to the GPU in a single Apply() call instead of one partial upload per block.
            int textureWidth = ((Texture2D)radiationSuitRenderer.material.mainTexture).width;
            Color[] fullPixels = radiationSuitRenderer.material.GetSourcePixels();
            fullPixels.InsertBlock(textureWidth, legTextureBlock, legPixels);
            fullPixels.InsertBlock(textureWidth, feetTextureBlock, feetPixels);
            fullPixels.InsertBlock(textureWidth, beltTextureBlock, beltPixels);

            radiationSuitRenderer.material.UpdateMainTextureColors(fullPixels);
            radiationSuitRenderer.materials[1].UpdateMainTextureColors(armSleevesPixels);

            radiationSuitRenderer.material.SetTexture("_MainText", radiationSuitRenderer.material.mainTexture);
            radiationSuitRenderer.material.SetTexture("_SpecTex", radiationSuitRenderer.material.mainTexture);
            radiationSuitRenderer.materials[1].SetTexture("_MainText", radiationSuitRenderer.materials[1].mainTexture);
            radiationSuitRenderer.materials[1].SetTexture("_SpecTex", radiationSuitRenderer.materials[1].mainTexture);
        }
    }
}

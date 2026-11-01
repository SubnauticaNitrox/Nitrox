using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
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

        public void ApplyPlayerColor(INitroxPlayer nitroxPlayer)
        {
            GameObject playerModel = nitroxPlayer.PlayerModel;
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor.ToUnity();
            PlayerColorRenderTextures renderTextures = PlayerColorRenderTextures.GetOrAdd(playerModel);

            SkinnedMeshRenderer radiationSuitRenderer = playerModel.GetRenderer(RADIATION_SUIT_GAME_OBJECT_NAME);
            Texture2D mainTexture = (Texture2D)radiationSuitRenderer.material.mainTexture;

            RecolorRegion legRegion = RecolorRegion.ForBlock(legTextureBlock, mainTexture.width, mainTexture.height, ColorSwapMode.HueSaturationVibrancy)
                .WithSaturationRange(0f, 35f)
                .WithVibrancyRange(40f, 100f);
            RecolorRegion feetRegion = RecolorRegion.ForBlock(feetTextureBlock, mainTexture.width, mainTexture.height, ColorSwapMode.Hue)
                .WithHueRange(0f, 100f)
                .WithVibrancyRange(30f, 100f);
            RecolorRegion beltRegion = RecolorRegion.ForBlock(beltTextureBlock, mainTexture.width, mainTexture.height, ColorSwapMode.Hue)
                .WithHueRange(0f, 90f)
                .WithVibrancyRange(3f, 100f);
            RecolorRegion armRegion = RecolorRegion.FullTexture(ColorSwapMode.Hue)
                .WithHueRange(0f, 100f)
                .WithVibrancyRange(30f, 100f);

            RenderTexture mainRenderTexture = GpuRecolorer.Recolor(mainTexture, playerColor, legRegion, feetRegion, beltRegion);
            RenderTexture armRenderTexture = GpuRecolorer.Recolor((Texture2D)radiationSuitRenderer.materials[1].mainTexture, playerColor, armRegion);

            radiationSuitRenderer.material.mainTexture = mainRenderTexture;
            radiationSuitRenderer.material.SetTexture("_SpecTex", mainRenderTexture);
            radiationSuitRenderer.materials[1].mainTexture = armRenderTexture;
            radiationSuitRenderer.materials[1].SetTexture("_SpecTex", armRenderTexture);

            renderTextures.Track(mainRenderTexture);
            renderTextures.Track(armRenderTexture);
        }
    }
}

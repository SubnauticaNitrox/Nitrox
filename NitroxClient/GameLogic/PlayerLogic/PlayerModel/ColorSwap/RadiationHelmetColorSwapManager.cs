using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using UnityEngine;
using static NitroxClient.GameLogic.PlayerLogic.PlayerModel.PlayerEquipmentConstants;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap
{
    public class RadiationHelmetColorSwapManager : IColorSwapManager
    {
        public void ApplyPlayerColor(INitroxPlayer nitroxPlayer)
        {
            GameObject playerModel = nitroxPlayer.PlayerModel;
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor.ToUnity();
            PlayerColorRenderTextures renderTextures = PlayerColorRenderTextures.GetOrAdd(playerModel);

            RecolorRegion region = RecolorRegion.FullTexture(ColorSwapMode.HueSaturationVibrancy)
                .WithSaturationRange(0f, 35f)
                .WithVibrancyRange(30f, 100f);

            SkinnedMeshRenderer radiationHelmetRenderer = playerModel.GetRenderer(RADIATION_HELMET_GAME_OBJECT_NAME);
            RenderTexture helmetTexture = GpuRecolorer.Recolor((Texture2D)radiationHelmetRenderer.material.mainTexture, playerColor, region);
            radiationHelmetRenderer.material.mainTexture = helmetTexture;
            renderTextures.Track(helmetTexture);

            SkinnedMeshRenderer radiationSuitNeckClaspRenderer = playerModel.GetRenderer(RADIATION_SUIT_NECK_CLASP_GAME_OBJECT_NAME);
            RenderTexture neckClaspTexture = GpuRecolorer.Recolor((Texture2D)radiationSuitNeckClaspRenderer.material.mainTexture, playerColor, region);
            radiationSuitNeckClaspRenderer.material.mainTexture = neckClaspTexture;
            renderTextures.Track(neckClaspTexture);
        }
    }
}

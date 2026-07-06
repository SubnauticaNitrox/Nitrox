using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using UnityEngine;
using static NitroxClient.GameLogic.PlayerLogic.PlayerModel.PlayerEquipmentConstants;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap
{
    public class RadiationSuitVestColorSwapManager : IColorSwapManager
    {
        public void ApplyPlayerColor(INitroxPlayer nitroxPlayer)
        {
            GameObject playerModel = nitroxPlayer.PlayerModel;
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor.ToUnity();
            PlayerColorRenderTextures renderTextures = PlayerColorRenderTextures.GetOrAdd(playerModel);

            SkinnedMeshRenderer radiationVestRenderer = playerModel.GetRenderer(RADIATION_SUIT_VEST_GAME_OBJECT_NAME);
            RenderTexture texture = GpuRecolorer.Recolor(
                (Texture2D)radiationVestRenderer.material.mainTexture,
                playerColor,
                RecolorRegion.FullTexture(ColorSwapMode.HueSaturationVibrancy)
                    .WithSaturationRange(0f, 35f)
                    .WithVibrancyRange(12f, 100f));

            radiationVestRenderer.material.mainTexture = texture;
            renderTextures.Track(texture);
        }
    }
}

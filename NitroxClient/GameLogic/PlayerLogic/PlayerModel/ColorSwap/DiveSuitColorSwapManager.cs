using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using UnityEngine;
using static NitroxClient.GameLogic.PlayerLogic.PlayerModel.PlayerEquipmentConstants;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap
{
    public class DiveSuitColorSwapManager : IColorSwapManager
    {
        public void ApplyPlayerColor(INitroxPlayer nitroxPlayer)
        {
            GameObject playerModel = nitroxPlayer.PlayerModel;
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor.ToUnity();
            PlayerColorRenderTextures renderTextures = PlayerColorRenderTextures.GetOrAdd(playerModel);

            SkinnedMeshRenderer diveSuitRenderer = playerModel.GetRenderer(DIVE_SUIT_GAME_OBJECT_NAME);
            RecolorRegion region = RecolorRegion.FullTexture(ColorSwapMode.Hue).WithHueRange(5f, 45f);

            RenderTexture bodyTexture = GpuRecolorer.Recolor((Texture2D)diveSuitRenderer.material.mainTexture, playerColor, region);
            RenderTexture armTexture = GpuRecolorer.Recolor((Texture2D)diveSuitRenderer.materials[1].mainTexture, playerColor, region);

            diveSuitRenderer.material.mainTexture = bodyTexture;
            diveSuitRenderer.materials[1].mainTexture = armTexture;

            renderTextures.Track(bodyTexture);
            renderTextures.Track(armTexture);
        }
    }
}

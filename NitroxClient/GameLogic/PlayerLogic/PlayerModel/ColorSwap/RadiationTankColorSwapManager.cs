using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using UnityEngine;
using static NitroxClient.GameLogic.PlayerLogic.PlayerModel.PlayerEquipmentConstants;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap
{
    public class RadiationTankColorSwapManager : IColorSwapManager
    {
        public void ApplyPlayerColor(INitroxPlayer nitroxPlayer)
        {
            GameObject playerModel = nitroxPlayer.PlayerModel;
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor.ToUnity();
            PlayerColorRenderTextures renderTextures = PlayerColorRenderTextures.GetOrAdd(playerModel);

            SkinnedMeshRenderer radiationTankRenderer = playerModel.GetRenderer(RADIATION_TANK_GAME_OBJECT_NAME);
            RenderTexture texture = GpuRecolorer.Recolor(
                (Texture2D)radiationTankRenderer.material.mainTexture,
                playerColor,
                RecolorRegion.FullTexture(ColorSwapMode.Hue).WithHueRange(0f, 85f));
            renderTextures.Track(texture);

            radiationTankRenderer.material.mainTexture = texture;
            radiationTankRenderer.material.SetTexture("_MainTex", texture);
            radiationTankRenderer.material.SetTexture("_SpecTex", texture);
        }
    }
}

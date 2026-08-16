using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using UnityEngine;
using static NitroxClient.GameLogic.PlayerLogic.PlayerModel.PlayerEquipmentConstants;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap
{
    public class ScubaTankColorSwapManager : IColorSwapManager
    {
        public void ApplyPlayerColor(INitroxPlayer nitroxPlayer)
        {
            GameObject playerModel = nitroxPlayer.PlayerModel;
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor.ToUnity();
            PlayerColorRenderTextures renderTextures = PlayerColorRenderTextures.GetOrAdd(playerModel);

            SkinnedMeshRenderer scubaTankRenderer = playerModel.GetRenderer(SCUBA_TANK_GAME_OBJECT_NAME);
            RenderTexture texture = GpuRecolorer.Recolor(
                (Texture2D)scubaTankRenderer.material.mainTexture,
                playerColor,
                RecolorRegion.FullTexture(ColorSwapMode.Hue).WithHueRange(0f, 30f));
            renderTextures.Track(texture);

            scubaTankRenderer.material.mainTexture = texture;
            scubaTankRenderer.material.SetTexture("_MainTex", texture);
            scubaTankRenderer.material.SetTexture("_SpecTex", texture);
        }
    }
}

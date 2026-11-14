using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using UnityEngine;
using static NitroxClient.GameLogic.PlayerLogic.PlayerModel.PlayerEquipmentConstants;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap
{
    public class FinColorSwapManager : IColorSwapManager
    {
        public void ApplyPlayerColor(INitroxPlayer nitroxPlayer)
        {
            GameObject playerModel = nitroxPlayer.PlayerModel;
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor.ToUnity();
            PlayerColorRenderTextures renderTextures = PlayerColorRenderTextures.GetOrAdd(playerModel);

            //All fin models use the same texture.
            SkinnedMeshRenderer basicFinRenderer = playerModel.GetRenderer(FINS_GAME_OBJECT_NAME);
            RenderTexture texture = GpuRecolorer.Recolor(
                (Texture2D)basicFinRenderer.material.mainTexture,
                playerColor,
                RecolorRegion.FullTexture(ColorSwapMode.Hue).WithHueRange(0f, 35f));
            renderTextures.Track(texture);

            basicFinRenderer.material.mainTexture = texture;
            basicFinRenderer.material.SetTexture("_MainTex", texture);
            basicFinRenderer.material.SetTexture("_SpecTex", texture);

            SkinnedMeshRenderer chargedFinRenderer = playerModel.GetRenderer(CHARGED_FINS_GAME_OBJECT_NAME);
            chargedFinRenderer.material.mainTexture = texture;
            chargedFinRenderer.material.SetTexture("_MainTex", texture);
            chargedFinRenderer.material.SetTexture("_SpecTex", texture);

            SkinnedMeshRenderer glideFinRenderer = playerModel.GetRenderer(GLIDE_FINS_GAME_OBJECT_NAME);
            glideFinRenderer.material.mainTexture = texture;
            glideFinRenderer.material.SetTexture("_MainTex", texture);
            glideFinRenderer.material.SetTexture("_SpecTex", texture);
        }
    }
}

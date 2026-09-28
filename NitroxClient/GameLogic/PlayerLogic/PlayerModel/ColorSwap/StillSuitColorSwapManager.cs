using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using UnityEngine;
using static NitroxClient.GameLogic.PlayerLogic.PlayerModel.PlayerEquipmentConstants;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap
{
    public class StillSuitColorSwapManager : IColorSwapManager
    {
        public void ApplyPlayerColor(INitroxPlayer nitroxPlayer)
        {
            GameObject playerModel = nitroxPlayer.PlayerModel;
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor.ToUnity();
            PlayerColorRenderTextures renderTextures = PlayerColorRenderTextures.GetOrAdd(playerModel);

            RecolorRegion region = RecolorRegion.FullTexture(ColorSwapMode.Hue).WithHueRange(0f, 75f);

            SkinnedMeshRenderer stillSuitRenderer = playerModel.GetRenderer(STILL_SUIT_GAME_OBJECT_NAME);
            RenderTexture bodyTexture = GpuRecolorer.Recolor((Texture2D)stillSuitRenderer.material.mainTexture, playerColor, region);
            renderTextures.Track(bodyTexture);
            stillSuitRenderer.material.mainTexture = bodyTexture;
            stillSuitRenderer.material.SetTexture("_MainTex", bodyTexture);
            stillSuitRenderer.material.SetTexture("_SpecTex", bodyTexture);

            RenderTexture armsTexture = GpuRecolorer.Recolor((Texture2D)stillSuitRenderer.materials[1].mainTexture, playerColor, region);
            renderTextures.Track(armsTexture);
            stillSuitRenderer.materials[1].mainTexture = armsTexture;
            stillSuitRenderer.materials[1].SetTexture("_MainTex", armsTexture);
            stillSuitRenderer.materials[1].SetTexture("_SpecTex", armsTexture);
        }
    }
}

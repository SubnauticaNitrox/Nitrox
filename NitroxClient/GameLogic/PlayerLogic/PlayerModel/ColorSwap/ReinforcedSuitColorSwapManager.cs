using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using UnityEngine;
using static NitroxClient.GameLogic.PlayerLogic.PlayerModel.PlayerEquipmentConstants;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap
{
    public class ReinforcedSuitColorSwapManager : IColorSwapManager
    {
        public void ApplyPlayerColor(INitroxPlayer nitroxPlayer)
        {
            GameObject playerModel = nitroxPlayer.PlayerModel;
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor.ToUnity();
            PlayerColorRenderTextures renderTextures = PlayerColorRenderTextures.GetOrAdd(playerModel);

            RecolorRegion region = RecolorRegion.FullTexture(ColorSwapMode.Hue)
                .WithHueRange(0f, 20f)
                .WithSaturationRange(45f, 100f);

            SkinnedMeshRenderer reinforcedSuitRenderer = playerModel.GetRenderer(REINFORCED_SUIT_GAME_OBJECT_NAME);
            RenderTexture suitTexture = GpuRecolorer.Recolor((Texture2D)reinforcedSuitRenderer.material.mainTexture, playerColor, region);
            renderTextures.Track(suitTexture);
            reinforcedSuitRenderer.material.mainTexture = suitTexture;
            reinforcedSuitRenderer.material.SetTexture("_MainTex", suitTexture);
            reinforcedSuitRenderer.material.SetTexture("_SpecTex", suitTexture);

            RenderTexture armsTexture = GpuRecolorer.Recolor((Texture2D)reinforcedSuitRenderer.materials[1].mainTexture, playerColor, region);
            renderTextures.Track(armsTexture);
            reinforcedSuitRenderer.materials[1].mainTexture = armsTexture;
            reinforcedSuitRenderer.materials[1].SetTexture("_MainTex", armsTexture);
            reinforcedSuitRenderer.materials[1].SetTexture("_SpecTex", armsTexture);

            SkinnedMeshRenderer reinforcedGloveRenderer = playerModel.GetRenderer(REINFORCED_GLOVES_GAME_OBJECT_NAME);
            RenderTexture gloveTexture = GpuRecolorer.Recolor((Texture2D)reinforcedGloveRenderer.material.mainTexture, playerColor, region);
            renderTextures.Track(gloveTexture);
            reinforcedGloveRenderer.material.mainTexture = gloveTexture;
        }
    }
}

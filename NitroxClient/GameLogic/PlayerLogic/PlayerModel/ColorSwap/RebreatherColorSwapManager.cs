using System.Collections.Generic;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using UnityEngine;
using static NitroxClient.GameLogic.PlayerLogic.PlayerModel.PlayerEquipmentConstants;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap
{
    public class RebreatherColorSwapManager : IColorSwapManager
    {
        public void ApplyPlayerColor(INitroxPlayer nitroxPlayer)
        {
            GameObject playerModel = nitroxPlayer.PlayerModel;
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor.ToUnity();
            PlayerColorRenderTextures renderTextures = PlayerColorRenderTextures.GetOrAdd(playerModel);

            SkinnedMeshRenderer rebreatherRenderer = playerModel.GetRenderer(REBREATHER_GAME_OBJECT_NAME);
            FixRebreatherMaterials(playerModel, rebreatherRenderer);

            RenderTexture texture = GpuRecolorer.Recolor(
                (Texture2D)rebreatherRenderer.material.mainTexture,
                playerColor,
                RecolorRegion.FullTexture(ColorSwapMode.Hue).WithHueRange(0f, 25f));
            renderTextures.Track(texture);

            rebreatherRenderer.material.mainTexture = texture;
        }

        //Clean up of UWE's tech debt from when they gave up on rendering player equipment on the avatar during normal play. Probably best not to read too much into it...
        private static void FixRebreatherMaterials(GameObject playerModel, SkinnedMeshRenderer rebreatherRenderer)
        {
            Shader marmosetShader = playerModel.GetRenderer(NORMAL_HEAD_GAME_OBJECT_NAME).material.shader;
            rebreatherRenderer.material.shader = marmosetShader;
            rebreatherRenderer.material.SetOverrideTag("RenderType", "TransparentAdditive");
            rebreatherRenderer.material.SetOverrideTag("Queue", "Deferred");
            rebreatherRenderer.material.shaderKeywords = new List<string>
                {"MARMO_ALPHA", "MARMO_PREMULT_ALPHA", "MARMO_SIMPLE_GLASS", "UWE_DITHERALPHA", "MARMO_SPECMAP", "WBOIT", "_NORMALMAP", "_ZWRITE_ON"}.ToArray();

            rebreatherRenderer.material.SetTexture("_MainTex", rebreatherRenderer.material.mainTexture);
            rebreatherRenderer.material.SetTexture("_SpecTex", rebreatherRenderer.material.mainTexture);
            rebreatherRenderer.material.SetTexture("_BumpMap", rebreatherRenderer.material.GetTexture("_BumpMap"));

            rebreatherRenderer.materials[2].shader = marmosetShader;
            rebreatherRenderer.materials[2].shaderKeywords = new List<string>
                {"MARMO_SPECMAP", "_ZWRITE_ON"}.ToArray();
            rebreatherRenderer.materials[2].SetTexture("_MainTex", rebreatherRenderer.materials[2].mainTexture);
            rebreatherRenderer.materials[2].SetTexture("_SpecTex", rebreatherRenderer.materials[2].mainTexture);
        }
    }
}

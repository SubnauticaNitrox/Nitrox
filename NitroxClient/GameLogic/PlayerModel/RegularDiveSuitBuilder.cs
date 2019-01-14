using NitroxClient.GameLogic.PlayerModel.Abstract;
using NitroxClient.Unity.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModel
{
    public class RegularDiveSuitBuilder : IPlayerModelBuilder
    {
        private readonly GameObject modelGeometry;

        public RegularDiveSuitBuilder(GameObject modelGeometry)
        {
            this.modelGeometry = modelGeometry;
        }

        public void Build(INitroxPlayer player)
        {
            float playerHue;
            float playerSaturation;
            float playerVibrancy;

            Color.RGBToHSV(player.PlayerSettings.PlayerColor, out playerHue, out playerSaturation, out playerVibrancy);

            float minVibrancy = playerVibrancy >= 0.75f ? 23f : 15f;

            HsvColorFilter filter = new HsvColorFilter(playerHue, playerSaturation, playerVibrancy, -1f);
            filter.SetHueRange(5f, 45f);
            filter.SetSaturationRange(50f, 100f);
            filter.SetVibrancyRange(minVibrancy, 100f);

            GameObject diveSuit = modelGeometry.RequireGameObject("diveSuit");
            SkinnedMeshRenderer[] renderers = diveSuit.GetAllComponentsInChildren<SkinnedMeshRenderer>();

            Material torsoMaterial = renderers[0].materials[0];
            torsoMaterial.ApplyFiltersToMainTexture(filter);

            Material armsMaterial = renderers[0].materials[1];
            armsMaterial.ApplyFiltersToMainTexture(filter);
        }
    }
}

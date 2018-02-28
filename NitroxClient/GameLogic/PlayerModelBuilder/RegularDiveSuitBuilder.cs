using NitroxClient.GameLogic.PlayerModelBuilder.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModelBuilder
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
            float playerVibrance;

            Color.RGBToHSV(player.PlayerSettings.PlayerColor, out playerHue, out playerSaturation, out playerVibrance);

            float minVibrance = playerVibrance >= 0.75f
                ? 23f
                : 15f;

            HsvColorFilter filter = new HsvColorFilter(playerHue, playerSaturation, playerVibrance, -1f);
            filter.AddHueRange(5f, 45f);
            filter.AddSaturationRange(50f, 100f);
            filter.AddVibranceRange(minVibrance, 100f);

            GameObject diveSuit = modelGeometry.transform.Find("diveSuit").gameObject;
            SkinnedMeshRenderer[] renderers = diveSuit.GetAllComponentsInChildren<SkinnedMeshRenderer>();

            Material torsoMaterial = renderers[0].materials[0];
            torsoMaterial.ApplyFiltersToMainTexture(filter);

            Material armsMaterial = renderers[0].materials[1];
            armsMaterial.ApplyFiltersToMainTexture(filter);
        }
    }
}

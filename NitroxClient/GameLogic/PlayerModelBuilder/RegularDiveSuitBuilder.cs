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

            //Ignored because we simply need to retrieve the hue of the PlayerColor.
            float ignoredSaturation;
            float ignoredVibrance;

            Color.RGBToHSV(player.PlayerSettings.PlayerColor, out playerHue, out ignoredSaturation, out ignoredVibrance);

            HsvColorFilter filter = new HsvColorFilter(playerHue, -1f, -1f, -1f);
            filter.AddHueRange(0f, 60f);

            GameObject diveSuit = modelGeometry.transform.Find("diveSuit").gameObject;
            SkinnedMeshRenderer[] renderers = diveSuit.GetAllComponentsInChildren<SkinnedMeshRenderer>();

            Material torsoMaterial = renderers[0].materials[0];
            torsoMaterial.ApplyFiltersToMainTexture(filter);

            Material armsMaterial = renderers[0].materials[1];
            armsMaterial.ApplyFiltersToMainTexture(filter);
        }
    }
}

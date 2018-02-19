using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModelBuilder
{
    public class RegularDiveSuitBuilder : EquipmentModelBuilder
    {
        public RegularDiveSuitBuilder(GameObject modelGeometry)
            : base(modelGeometry)
        {
        }

        protected override void HandleBuild(RemotePlayer player)
        {
            GameObject diveSuit = ModelGeometry.transform.Find("diveSuit").gameObject;
            SkinnedMeshRenderer[] renderers = diveSuit.GetAllComponentsInChildren<SkinnedMeshRenderer>();

            Material torsoMaterial = renderers[0].materials[0];
            Material armsMaterial = renderers[0].materials[1];

            float playerHue;

            //Ignored because we simply need to retrieve the hue of the PlayerColor.
            float ignoredSaturation;
            float ignoredVibrance;

            Color.RGBToHSV(player.PlayerSettings.PlayerColor, out playerHue, out ignoredSaturation, out ignoredVibrance);

            HsvColorFilter filter = new HsvColorFilter(playerHue, -1f, -1f, -1f);
            filter.AddHueRange(0f, 60f / 360f);

            PaintMaterial(torsoMaterial, filter);
            PaintMaterial(armsMaterial, filter);
        }
    }
}

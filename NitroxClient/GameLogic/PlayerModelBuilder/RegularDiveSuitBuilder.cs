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
            HsvColorFilter filter = new HsvColorFilter(player.PlayerSettings.PlayerColor);
            filter.AddHueRange(0f, 60f / 360f);

            PaintMaterial(torsoMaterial, filter);
            PaintMaterial(armsMaterial, filter);
        }
    }
}

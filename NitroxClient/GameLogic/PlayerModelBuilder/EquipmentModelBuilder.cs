using NitroxClient.GameLogic.PlayerModelBuilder.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModelBuilder
{
    // This base class has some handy methods that are common to bulding all types of equipment.
    public abstract class EquipmentModelBuilder : BasePlayerModelBuildHandler
    {
        protected EquipmentModelBuilder(GameObject modelGeometry)
        {
            ModelGeometry = modelGeometry;
        }

        protected GameObject ModelGeometry { get; set; }

        // We can swap this out with a method that applies the coloring effect with a custom shader sometime after 1.0
        protected void PaintMaterial(Material material, params HsvColorFilter[] filters)
        {
            Texture2D texture = (Texture2D)material.mainTexture;
            Texture2D readableTexture = texture.Clone();
            readableTexture.ApplyFilters(filters);

            material.mainTexture = readableTexture;
        }
    }
}

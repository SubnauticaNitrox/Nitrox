using NitroxClient.GameLogic.PlayerModelBuilder.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModelBuilder
{
    //This base class has some handy methods that are common to bulding all types of equipment. 
    public abstract class EquipmentModelBuilder : BasePlayerModelBuildHandler
    {
        protected GameObject ModelGeometry { get; set; }

        protected EquipmentModelBuilder(GameObject modelGeometry)
        {
            ModelGeometry = modelGeometry;
        }

        //We can swap this out with a method that applies the coloring effect with a custom shader sometime after 1.0 
        protected void PaintMaterial(Material material, params HsvColorFilter[] filters)
        {
            Texture2D texture = (Texture2D)material.mainTexture;
            Texture2D readableTexture = CloneReadableTexture(texture);
            material.mainTexture = readableTexture;

            Color[] pixels = readableTexture.GetPixels();

            for (int index = 0; index < pixels.Length; index++)
            {
                Color currentPixel = pixels[index];

                foreach (HsvColorFilter filter in filters)
                {
                    Color filteredPixel = filter.FilterColor(currentPixel);

                    if (filteredPixel != currentPixel)
                    {
                        pixels[index] = filteredPixel;
                        break;
                    }
                }
            }

            readableTexture.SetPixels(pixels);
            readableTexture.Apply();
        }

        //This entire method is necessary in order to deal with the fact that UWE compiles Subnautica in a mode 
        //that prevents us from accessing the pixel map of the 2D textures they apply to their materials. 
        private Texture2D CloneReadableTexture(Texture2D textureToClone)
        {
            // Create a temporary RenderTexture of the same size as the texture
            RenderTexture tmp = RenderTexture.GetTemporary(
                textureToClone.width,
                textureToClone.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(textureToClone, tmp);
            // Backup the currently set RenderTexture
            RenderTexture previous = RenderTexture.active;
            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;
            // Create a new readable Texture2D to copy the pixels to it
            Texture2D clonedTexture = new Texture2D(textureToClone.width, textureToClone.height);
            // Copy the pixels from the RenderTexture to the new Texture
            clonedTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            clonedTexture.Apply();
            // Reset the active RenderTexture
            RenderTexture.active = previous;
            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);

            return clonedTexture;
            // "clonedTexture" now has the same pixels from "texture" and it's readable.
        }
    }
}

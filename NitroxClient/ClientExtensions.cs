using NitroxClient.GameLogic.PlayerModelBuilder;
using UnityEngine;

namespace NitroxClient
{
    public static class ClientExtensions
    {
        //This entire method is necessary in order to deal with the fact that UWE compiles Subnautica in a mode 
        //that prevents us from accessing the pixel map of the 2D textures they apply to their materials. 
        public static Texture2D Clone(this Texture2D sourceTexture)
        {
            // Create a temporary RenderTexture of the same size as the texture
            RenderTexture tmp = RenderTexture.GetTemporary(
                sourceTexture.width,
                sourceTexture.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(sourceTexture, tmp);
            // Backup the currently set RenderTexture
            RenderTexture previous = RenderTexture.active;
            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;
            // Create a new readable Texture2D to copy the pixels to it
            Texture2D clonedTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
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

        public static void ApplyFilters(this Texture2D texture, params HsvColorFilter[] filters)
        {
            Color[] pixels = texture.GetPixels();
            FilterPixels(filters, pixels);

            texture.SetPixels(pixels);
            texture.Apply();
        }

        //This applies a color filter to a specific region of a 2D texture.
        public static void ApplyFiltersToBlock(
            this Texture2D texture,
            int x,
            int y,
            int blockWidth,
            int blockHeight,
            params HsvColorFilter[] filters)
        {
            Color[] pixels = texture.GetPixels(x, y, blockWidth, blockHeight);
            FilterPixels(filters, pixels);

            texture.SetPixels(x, y, blockWidth, blockHeight, pixels);
            texture.Apply();
        }

        private static void FilterPixels(HsvColorFilter[] filters, Color[] pixels)
        {
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
        }
    }
}

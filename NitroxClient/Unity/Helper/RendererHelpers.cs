using NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap;
using UnityEngine;

namespace NitroxClient.Unity.Helper
{
    public static class RendererHelpers
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

        //This applies a color filter to a specific region of a 2D texture.
        public static void SwapTextureColors(
            this Texture2D texture,
            HsvSwapper filter,
            TextureBlock textureBlock)
        {
            Color[] pixels = texture.GetPixels(textureBlock.X, textureBlock.Y, textureBlock.BlockWidth, textureBlock.BlockHeight);

            filter.SwapColors(pixels);

            texture.SetPixels(textureBlock.X, textureBlock.Y, textureBlock.BlockWidth, textureBlock.BlockHeight, pixels);
            texture.Apply();
        }

        public static void UpdateMainTextureColors(this Material material, Color[] pixels)
        {
            Texture2D mainTexture = (Texture2D)material.mainTexture;
            mainTexture.SetPixels(pixels);
            mainTexture.Apply();
        }

        //This applies a color filter to a specific region of a 2D texture.
        public static void UpdateMainTextureColors(
            this Material material,
            Color[] pixels,
            //IColorSwapStrategy colorSwapStrategy,
            TextureBlock textureBlock)
        {
            Texture2D mainTexture = (Texture2D)material.mainTexture;
            //Color[] pixels = mainTexture.GetPixels(textureBlock.X, textureBlock.Y, textureBlock.BlockWidth, textureBlock.BlockHeight);
            //pixelIndexes.ForEach(pixelIndex => pixels[pixelIndex] = colorSwapStrategy.SwapColor(pixels[pixelIndex]));
            mainTexture.SetPixels(textureBlock.X, textureBlock.Y, textureBlock.BlockWidth, textureBlock.BlockHeight, pixels);
            mainTexture.Apply();
        }

        public static void ApplyClonedTexture(this Material material)
        {
            Texture2D mainTexture = (Texture2D)material.mainTexture;
            Texture2D clonedTexture = mainTexture.Clone();
            material.mainTexture = clonedTexture;
        }

        public static SkinnedMeshRenderer GetRenderer(this GameObject playerModel, string equipmentGameObjectName)
        {
            return playerModel
                .transform
                .Find(equipmentGameObjectName)
                .gameObject
                .GetComponent<SkinnedMeshRenderer>();
        }

        public static Color[] GetMainTexturePixels(this Material material)
        {
            Texture2D mainTexture = (Texture2D)material.mainTexture;
            return mainTexture.GetPixels();
        }

        public static Color[] GetMainTexturePixelBlock(
            this Material material,
            TextureBlock textureBlock)
        {
            Texture2D mainTexture = (Texture2D)material.mainTexture;
            return mainTexture.GetPixels(textureBlock.X, textureBlock.Y, textureBlock.BlockWidth, textureBlock.BlockHeight);
        }
    }
}

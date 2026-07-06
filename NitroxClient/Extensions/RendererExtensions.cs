using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace NitroxClient.Extensions;

public static class RendererExtensions
{
    // Every remote player model is a clone of the same prototype prefab, so the source Texture2D of a given
    // body part is the exact same asset for every player. Caching its readback avoids repeating the expensive
    // Graphics.Blit + ReadPixels GPU/CPU sync stall for every single player join.
    private static readonly Dictionary<Texture2D, Color[]> sourceTexturePixelCache = new();

    //This entire method is necessary in order to deal with the fact that UWE compiles Subnautica in a mode
    //that prevents us from accessing the pixel map of the 2D textures they apply to their materials.
    //Returns a private copy of the pixel data (from cache when possible) so callers can freely mutate it,
    //e.g. for a hue swap, without corrupting the cached data shared by every other player.
    public static Color[] GetSourcePixels(this Texture2D sourceTexture)
    {
        if (sourceTexturePixelCache.TryGetValue(sourceTexture, out Color[] cachedPixels))
        {
            return (Color[])cachedPixels.Clone();
        }

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

        // This scratch texture only exists to pull the pixels off the GPU into a Color[]; it's never assigned
        // to a material or rendered, so there's no need to Apply() it (which would upload it back to the GPU
        // and rebuild mipmaps for nothing).
        Texture2D scratchTexture = new(sourceTexture.width, sourceTexture.height);
        scratchTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);

        // Reset the active RenderTexture
        RenderTexture.active = previous;
        // Release the temporary RenderTexture
        RenderTexture.ReleaseTemporary(tmp);

        Color[] pixels = scratchTexture.GetPixels();
        UnityEngine.Object.Destroy(scratchTexture);

        sourceTexturePixelCache[sourceTexture] = pixels;

        return (Color[])pixels.Clone();
    }

    public static Color[] GetSourcePixels(this Material material)
    {
        return ((Texture2D)material.mainTexture).GetSourcePixels();
    }

    /// <summary>
    /// Kicks off an <see cref="AsyncGPUReadback"/> request for every not-yet-cached texture in
    /// <paramref name="sourceTextures"/> up front (in parallel), then yields until they all complete, populating
    /// <see cref="sourceTexturePixelCache"/> for each. Unlike <see cref="GetSourcePixels(Texture2D)"/>'s
    /// synchronous Blit+ReadPixels fallback, this never blocks the main thread waiting on the GPU - the wait is
    /// spread across however many frames the readback actually takes.
    /// </summary>
    public static IEnumerator PrewarmSourcePixelsAsync(this IEnumerable<Texture2D> sourceTextures)
    {
        List<(Texture2D texture, RenderTexture renderTexture, AsyncGPUReadbackRequest request)> pending = new();

        foreach (Texture2D sourceTexture in sourceTextures.Distinct())
        {
            if (sourceTexturePixelCache.ContainsKey(sourceTexture))
            {
                continue;
            }

            RenderTexture tmp = RenderTexture.GetTemporary(
                sourceTexture.width,
                sourceTexture.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);
            Graphics.Blit(sourceTexture, tmp);

            pending.Add((sourceTexture, tmp, AsyncGPUReadback.Request(tmp, 0, TextureFormat.RGBA32)));
        }

        if (pending.Count == 0)
        {
            yield break;
        }

        while (pending.Any(p => !p.request.done))
        {
            yield return null;
        }

        // Direct3D/Metal/consoles store the readback rows top-to-bottom, while Texture2D.GetPixels()/SetPixels()
        // (and therefore our cache) always use Unity's bottom-to-top convention - flip rows to match when needed.
        bool needsRowFlip = SystemInfo.graphicsUVStartsAtTop;

        foreach ((Texture2D sourceTexture, RenderTexture tmp, AsyncGPUReadbackRequest request) in pending)
        {
            if (request.hasError)
            {
                Log.Error($"AsyncGPUReadback failed for '{sourceTexture.name}', falling back to synchronous readback");
                sourceTexture.GetSourcePixels();
            }
            else
            {
                int width = sourceTexture.width;
                int height = sourceTexture.height;
                NativeArray<Color32> data = request.GetData<Color32>();
                Color[] pixels = new Color[data.Length];
                for (int y = 0; y < height; y++)
                {
                    int srcRow = needsRowFlip ? height - 1 - y : y;
                    for (int x = 0; x < width; x++)
                    {
                        pixels[y * width + x] = data[srcRow * width + x];
                    }
                }
                sourceTexturePixelCache[sourceTexture] = pixels;
            }

            RenderTexture.ReleaseTemporary(tmp);
        }
    }

    public static Color[] GetSourcePixelBlock(this Material material, TextureBlock textureBlock)
    {
        Texture2D mainTexture = (Texture2D)material.mainTexture;
        return mainTexture.GetSourcePixels().ExtractBlock(mainTexture.width, textureBlock);
    }

    //Extracts a sub-rectangle from a full-texture pixel array, mirroring Texture2D.GetPixels(x, y, w, h).
    public static Color[] ExtractBlock(this Color[] fullPixels, int textureWidth, TextureBlock textureBlock)
    {
        Color[] block = new Color[textureBlock.BlockWidth * textureBlock.BlockHeight];
        for (int row = 0; row < textureBlock.BlockHeight; row++)
        {
            Array.Copy(fullPixels, (textureBlock.Y + row) * textureWidth + textureBlock.X, block, row * textureBlock.BlockWidth, textureBlock.BlockWidth);
        }
        return block;
    }

    //Patches a sub-rectangle back into a full-texture pixel array, mirroring Texture2D.SetPixels(x, y, w, h, ...).
    public static void InsertBlock(this Color[] fullPixels, int textureWidth, TextureBlock textureBlock, Color[] blockPixels)
    {
        for (int row = 0; row < textureBlock.BlockHeight; row++)
        {
            Array.Copy(blockPixels, row * textureBlock.BlockWidth, fullPixels, (textureBlock.Y + row) * textureWidth + textureBlock.X, textureBlock.BlockWidth);
        }
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

    //Builds the final per-player texture with the already-swapped pixels baked in and assigns it to the
    //material in a single upload, instead of uploading an intermediate unswapped copy first.
    public static void UpdateMainTextureColors(this Material material, Color[] pixels)
    {
        Texture2D sourceTexture = (Texture2D)material.mainTexture;
        Texture2D newTexture = new(sourceTexture.width, sourceTexture.height);
        newTexture.SetPixels(pixels);
        newTexture.Apply();
        material.mainTexture = newTexture;
    }

    public static SkinnedMeshRenderer GetRenderer(this GameObject playerModel, string equipmentGameObjectName)
    {
        return playerModel
               .transform
               .Find(equipmentGameObjectName)
               .gameObject
               .GetComponent<SkinnedMeshRenderer>();
    }

    /// Copied from MainMenuLoadButton.ShiftAlpha()
    public static IEnumerator ShiftAlpha(
        this CanvasGroup cg,
        float targetAlpha,
        float animTime,
        float power,
        bool toActive,
        Selectable buttonToSelect = null)
    {
        float start = Time.time;
        while (Time.time - start < animTime)
        {
            cg.alpha = Mathf.Lerp(cg.alpha, targetAlpha, Mathf.Pow(Mathf.Clamp01((Time.time - start) / animTime), power));
            yield return null;
        }
        cg.alpha = targetAlpha;
        if (toActive)
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
        else
        {
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
    }
}

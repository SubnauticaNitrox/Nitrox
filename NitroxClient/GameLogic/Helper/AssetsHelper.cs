using System;
using System.Collections.Generic;
using NitroxClient.Unity.Helper;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace NitroxClient.GameLogic.Helper;

public class AssetsHelper
{
    private static readonly Dictionary<Atlas.Sprite, Atlas.Sprite> cachedSprites = new();

    private static Dictionary<string, Texture2D> playerListTabImages = new();
    private static bool loadingBundle = false;
    public static bool AssetBundleLoaded = false;
    public static OnPlayerListAssetsLoaded onPlayerListAssetsLoaded;

    public static void Initialize()
    {
        if (!loadingBundle && !AssetBundleLoaded)
        {
            LoadPlayerListAssets();
        }
    }

    private static void LoadPlayerListAssets()
    {
        // This must happen asap so that this function is never called twice
        loadingBundle = true;
        Player.main.StartCoroutine(AssetBundleLoader.LoadAssetBundle("playerlisttab", bundle =>
        {
            Log.Debug("LoadButtonsImages: " + bundle.name);
            object[] assets = bundle.LoadAllAssets();
            foreach (object asset in assets)
            {
                if (asset is Texture2D texture)
                {
                    playerListTabImages.Add(texture.name, texture);
                    Log.Debug($"Loaded texture : {texture.name}");
                }
            }
            loadingBundle = false;
            AssetBundleLoaded = true;
            if (onPlayerListAssetsLoaded != null)
            { 
                onPlayerListAssetsLoaded();
            }
        }));
    }

    public static Texture2D GetTexture(string assetName)
    {
        if (playerListTabImages.TryGetValue(assetName, out Texture2D texture))
        {
            Log.Debug($"return texture {texture.name}");
            return texture;
        }
        return new Texture2D(100, 100);
    }

    public static Sprite MakeSpriteFromTexture(string assetName)
    {
        Texture2D tex = GetTexture(assetName);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2, tex.height / 2), 100);
    }

    public static Atlas.Sprite MakeAtlasSpriteFromTexture(string assetName)
    {
        Texture2D tex = GetTexture(assetName);
        return new Atlas.Sprite(tex);
    }

    // TODO: Old way of building the texture, remove when merging
    public static Atlas.Sprite CreateSprite(Atlas.Sprite baseSprite)
    {
        if (cachedSprites.TryGetValue(baseSprite, out Atlas.Sprite cachedSprite))
        {
            return cachedSprite;
        }

        Texture2D oldTex = GetAsReadableTexture(baseSprite.texture);
        Texture2D newTex = new(oldTex.width, oldTex.height, oldTex.graphicsFormat, oldTex.mipmapCount, TextureCreationFlags.None);

        Texture2D[] layers = new Texture2D[3] { oldTex, oldTex, oldTex };
        Color[] colorArray = new Color[oldTex.width * oldTex.height];
        Color[][] srcArray = new Color[layers.Length][];

        for (int i = 0; i < layers.Length; i++)
        {
            // Color array starts from bottom left and goes from left to right, row by row
            Color[] pixels = layers[i].GetPixels();
            Color[] newArray = new Color[layers[i].GetPixels().Length];

            int scale = 3;
            int xOffset = scale * -3 * (i - 1);
            int yOffset = scale * -2 * (i - 1);

            int yPixelsOffset = yOffset * oldTex.width;
            int xPixelsOffset = xOffset * oldTex.height;
            // Debug.Log($"[i={i}] xOffset={xOffset}, yPixelsOffset:{yPixelsOffset}, yOffset={yOffset}, totalPixel: {pixels.Length}");

            for (int j = 0; j < pixels.Length; j++)
            {
                int pixelX = j % oldTex.width;
                int pixelY = (j - pixelX) / oldTex.width;
                // These lines usually cause some weird blur at each corner of the image
                if (pixelX == 0 || pixelX == oldTex.width - 1 || pixelY == 0 || pixelY == oldTex.height - 1)
                {
                    newArray[j] = new Color(0, 0, 0, 0);
                }
                // Check for y axis translation
                else if (j - yPixelsOffset - xOffset < 0 || j - yPixelsOffset - xOffset >= pixels.Length)
                {
                    newArray[j] = new Color(0, 0, 0, 0);
                }
                else
                {
                    // Check for x axis translation
                    if (0 <= pixelX - xOffset && pixelX - xOffset <= oldTex.width)
                    {
                        newArray[j] = pixels[j - yPixelsOffset - xOffset];
                    }
                    else
                    {
                        newArray[j] = new Color(0, 0, 0, 0);
                    }
                }
            }
            srcArray[i] = newArray;
        }

        // This could maybe done before, by comparing if the previously set pixel is brighter
        for (int x = 0; x < oldTex.width; x++)
        {
            for (int y = 0; y < oldTex.height; y++)
            {
                int pixelIndex = x + (y * oldTex.width);
                for (int i = 0; i < layers.Length; i++)
                {
                    Color srcPixel = srcArray[i][pixelIndex];
                    if (srcPixel.a > 0.2)
                    {
                        colorArray[pixelIndex] = srcPixel;
                    }
                }
            }
        }

        newTex.SetPixels(colorArray);
        newTex.Apply();

        Atlas.Sprite newSprite = new(newTex);
        newSprite.size = baseSprite.size;
        newSprite.uv0 = baseSprite.uv0;
        newSprite.border = baseSprite.border;
        newSprite.slice9Grid = baseSprite.slice9Grid;
        newSprite.outer = baseSprite.outer;
        newSprite.pixelsPerUnit = baseSprite.pixelsPerUnit;
        newSprite.inner = baseSprite.inner;
        newSprite.triangles = baseSprite.triangles;
        newSprite.vertices = baseSprite.vertices;

        cachedSprites[baseSprite] = newSprite;
        return newSprite;
    }

    public static Texture2D GetAsReadableTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new(source.width, source.height, source.graphicsFormat, source.mipmapCount, TextureCreationFlags.None);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);

        return readableText;
    }

    public delegate void OnPlayerListAssetsLoaded();
}

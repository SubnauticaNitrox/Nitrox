using System;
using System.Collections;
using UnityEngine;
using static NitroxClient.Unity.Helper.AssetBundleLoader;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap;

/// <summary>
/// GPU replacement for the CPU pixel-loop recolor pipeline (formerly HsvSwapper + RendererExtensions'
/// GetSourcePixels/UpdateMainTextureColors). <see cref="Recolor"/> Blits a source texture through
/// Assets/colorswap/Recolor.shader, which performs the same per-region HSV range check and hue/hue+sat+vib
/// replacement entirely on the GPU - no ReadPixels/SetPixels/Apply() CPU roundtrip.
/// </summary>
public static class GpuRecolorer
{
    public const int MAX_REGIONS = 4;

    private static readonly int RegionRectId = Shader.PropertyToID("_RegionRect");
    private static readonly int RegionHueRangeId = Shader.PropertyToID("_RegionHueRange");
    private static readonly int RegionSatRangeId = Shader.PropertyToID("_RegionSatRange");
    private static readonly int RegionVibRangeId = Shader.PropertyToID("_RegionVibRange");
    private static readonly int RegionAlphaRangeId = Shader.PropertyToID("_RegionAlphaRange");
    private static readonly int RegionSwapModeId = Shader.PropertyToID("_RegionSwapMode");
    private static readonly int RegionCountId = Shader.PropertyToID("_RegionCount");
    private static readonly int ReplacementHsvId = Shader.PropertyToID("_ReplacementHSV");

    private static Material recolorMaterial;

    public static bool IsInitialized => recolorMaterial;

    public static void Initialize(Shader recolorShader)
    {
        recolorMaterial = new Material(recolorShader);
    }

    /// <summary>
    /// Lazily loads the colorswap AssetBundle and builds the shared recolor material the first time any
    /// caller needs it. Safe to call before every use; a no-op once already initialized.
    /// </summary>
    public static IEnumerator EnsureInitialized()
    {
        if (IsInitialized)
        {
            yield break;
        }

        yield return LoadAllAssets(NitroxAssetBundle.COLOR_SWAP);
        Initialize((Shader)NitroxAssetBundle.COLOR_SWAP.LoadedAssets[0]);
    }

    /// <summary>
    /// Recolors <paramref name="source"/> into a freshly allocated <see cref="RenderTexture"/> (caller assigns
    /// it as the material's mainTexture and is responsible for eventually releasing it, e.g. via
    /// <see cref="PlayerColorRenderTextures"/>).
    /// </summary>
    public static RenderTexture Recolor(Texture2D source, Color playerColor, params RecolorRegion[] regions)
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException($"{nameof(GpuRecolorer)} was not initialized before use.");
        }
        if (regions.Length > MAX_REGIONS)
        {
            throw new ArgumentOutOfRangeException(nameof(regions), $"At most {MAX_REGIONS} regions are supported per texture.");
        }

        Vector4[] rects = new Vector4[MAX_REGIONS];
        Vector4[] hueRanges = new Vector4[MAX_REGIONS];
        Vector4[] satRanges = new Vector4[MAX_REGIONS];
        Vector4[] vibRanges = new Vector4[MAX_REGIONS];
        Vector4[] alphaRanges = new Vector4[MAX_REGIONS];
        float[] swapModes = new float[MAX_REGIONS];

        for (int i = 0; i < regions.Length; i++)
        {
            RecolorRegion region = regions[i];
            rects[i] = region.UvRect;
            hueRanges[i] = region.HueRange;
            satRanges[i] = region.SaturationRange;
            vibRanges[i] = region.VibrancyRange;
            alphaRanges[i] = region.AlphaRange;
            swapModes[i] = (float)region.SwapMode;
        }

        Color.RGBToHSV(playerColor, out float hue, out float saturation, out float vibrancy);

        recolorMaterial.SetVectorArray(RegionRectId, rects);
        recolorMaterial.SetVectorArray(RegionHueRangeId, hueRanges);
        recolorMaterial.SetVectorArray(RegionSatRangeId, satRanges);
        recolorMaterial.SetVectorArray(RegionVibRangeId, vibRanges);
        recolorMaterial.SetVectorArray(RegionAlphaRangeId, alphaRanges);
        recolorMaterial.SetFloatArray(RegionSwapModeId, swapModes);
        recolorMaterial.SetInt(RegionCountId, regions.Length);
        recolorMaterial.SetVector(ReplacementHsvId, new Vector4(hue, saturation, vibrancy, 0f));

        RenderTexture renderTexture = new(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear)
        {
            name = $"Recolor_{source.name}"
        };
        Graphics.Blit(source, renderTexture, recolorMaterial);
        return renderTexture;
    }
}

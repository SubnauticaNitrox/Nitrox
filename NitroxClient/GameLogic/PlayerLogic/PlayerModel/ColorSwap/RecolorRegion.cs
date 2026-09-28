using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap;

public enum ColorSwapMode
{
    /// <summary>Replace hue only, keeping the original pixel's saturation/vibrancy. Mirrors <c>HueSwapper</c>.</summary>
    Hue = 0,

    /// <summary>Replace hue, saturation and vibrancy. Mirrors <c>HueSaturationVibrancySwapper</c>.</summary>
    HueSaturationVibrancy = 1
}

/// <summary>
/// Describes one rectangular region of a texture to recolor on the GPU: which sub-rect (whole texture, or a
/// <see cref="TextureBlock"/> in pixel space) and which HSV/alpha range within it counts as this equipment's
/// base color versus trim/shadow/etc. that should be left alone. Consumed by <see cref="GpuRecolorer"/>.
/// Range semantics (values above 1 are treated as degrees/percentages/0-255 and normalized) match what used to
/// live in HsvSwapper's SetHueRange/SetSaturationRange/SetVibrancyRange/SetAlphaRange, so existing call sites'
/// range values carry over unchanged.
/// </summary>
public readonly struct RecolorRegion
{
    private static readonly Vector4 FullRange = new(0f, 1f, 0f, 0f);

    public Vector4 UvRect { get; }
    public Vector4 HueRange { get; }
    public Vector4 SaturationRange { get; }
    public Vector4 VibrancyRange { get; }
    public Vector4 AlphaRange { get; }
    public ColorSwapMode SwapMode { get; }

    private RecolorRegion(Vector4 uvRect, Vector4 hueRange, Vector4 saturationRange, Vector4 vibrancyRange, Vector4 alphaRange, ColorSwapMode swapMode)
    {
        UvRect = uvRect;
        HueRange = hueRange;
        SaturationRange = saturationRange;
        VibrancyRange = vibrancyRange;
        AlphaRange = alphaRange;
        SwapMode = swapMode;
    }

    public static RecolorRegion FullTexture(ColorSwapMode swapMode)
    {
        return new RecolorRegion(new Vector4(0f, 0f, 1f, 1f), FullRange, FullRange, FullRange, FullRange, swapMode);
    }

    /// <summary>
    /// A region covering a pixel-space <see cref="TextureBlock"/> (bottom-left origin, matching
    /// <c>Texture2D.GetPixels(x, y, w, h)</c>) converted to the UV-space rect the shader expects.
    /// </summary>
    public static RecolorRegion ForBlock(TextureBlock block, int textureWidth, int textureHeight, ColorSwapMode swapMode)
    {
        Vector4 uvRect = new(
            block.X / (float)textureWidth,
            block.Y / (float)textureHeight,
            (block.X + block.BlockWidth) / (float)textureWidth,
            (block.Y + block.BlockHeight) / (float)textureHeight);
        return new RecolorRegion(uvRect, FullRange, FullRange, FullRange, FullRange, swapMode);
    }

    public RecolorRegion WithHueRange(float minHue, float maxHue) => new(UvRect, Normalize(minHue, maxHue, 360f, inclusive: false), SaturationRange, VibrancyRange, AlphaRange, SwapMode);

    public RecolorRegion WithSaturationRange(float minSaturation, float maxSaturation) => new(UvRect, HueRange, Normalize(minSaturation, maxSaturation, 100f, inclusive: true), VibrancyRange, AlphaRange, SwapMode);

    public RecolorRegion WithVibrancyRange(float minVibrancy, float maxVibrancy) => new(UvRect, HueRange, SaturationRange, Normalize(minVibrancy, maxVibrancy, 100f, inclusive: true), AlphaRange, SwapMode);

    public RecolorRegion WithAlphaRange(float minAlpha, float maxAlpha) => new(UvRect, HueRange, SaturationRange, VibrancyRange, Normalize(minAlpha, maxAlpha, 255f, inclusive: false), SwapMode);

    // inclusive controls whether a value of exactly 1 is treated as "already normalized" (sat/vib, matching
    // HsvSwapper's `<= 1f`) or as "1 degree/unit out of the raw scale" (hue/alpha, matching HsvSwapper's `< 1f`).
    private static Vector4 Normalize(float min, float max, float scale, bool inclusive)
    {
        float normalizedMin = (inclusive ? min <= 1f : min < 1f) ? min : min / scale;
        float normalizedMax = (inclusive ? max <= 1f : max < 1f) ? max : max / scale;
        return new Vector4(normalizedMin, normalizedMax, 0f, 0f);
    }
}

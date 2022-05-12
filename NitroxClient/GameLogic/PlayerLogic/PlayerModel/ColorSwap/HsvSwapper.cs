using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap
{
    public class HsvSwapper
    {
        private readonly IColorSwapStrategy colorSwapStrategy;
        private ColorValueRange alphaValueRange;
        private ColorValueRange hueValueRange;
        private ColorValueRange saturationValueRange;
        private ColorValueRange vibrancyValueRange;

        public HsvSwapper(IColorSwapStrategy colorSwapStrategy)
        {
            this.colorSwapStrategy = colorSwapStrategy;
            hueValueRange = new ColorValueRange(0f, 1f);
            saturationValueRange = new ColorValueRange(0f, 1f);
            vibrancyValueRange = new ColorValueRange(0f, 1f);
            alphaValueRange = new ColorValueRange(0f, 1f);
        }

        public void SetHueRange(float minHue, float maxHue)
        {
            float newFilterMinValue = minHue < 1f ? minHue : minHue / 360f;
            float newFilterMaxValue = maxHue < 1f ? maxHue : maxHue / 360f;

            hueValueRange = new ColorValueRange(newFilterMinValue, newFilterMaxValue);
        }

        public void SetSaturationRange(float minSaturation, float maxSaturation)
        {
            float newFilterMinValue = minSaturation <= 1f ? minSaturation : minSaturation / 100f;
            float newFilterMaxValue = maxSaturation <= 1f ? maxSaturation : maxSaturation / 100f;

            saturationValueRange = new ColorValueRange(newFilterMinValue, newFilterMaxValue);
        }

        public void SetVibrancyRange(float minVibrancy, float maxVibrancy)
        {
            float newFilterMinValue = minVibrancy <= 1f ? minVibrancy : minVibrancy / 100f;
            float newFilterMaxValue = maxVibrancy <= 1f ? maxVibrancy : maxVibrancy / 100f;

            vibrancyValueRange = new ColorValueRange(newFilterMinValue, newFilterMaxValue);
        }

        public void SetAlphaRange(float minAlpha, float maxAlpha)
        {
            float newFilterMinValue = minAlpha < 1f ? minAlpha : minAlpha / 255f;
            float newFilterMaxValue = maxAlpha < 1f ? maxAlpha : maxAlpha / 255f;

            alphaValueRange = new ColorValueRange(newFilterMinValue, newFilterMaxValue);
        }

        public void SwapColors(Color[] texturePixels)
        {
            for (int pixelIndex = 0; pixelIndex < texturePixels.Length; pixelIndex++)
            {
                Color pixel = texturePixels[pixelIndex];
                float alpha = pixel.a;

                Color.RGBToHSV(pixel, out float hue, out float saturation, out float vibrancy);

                if (hueValueRange.Covers(hue) &&
                    saturationValueRange.Covers(saturation) &&
                    vibrancyValueRange.Covers(vibrancy) &&
                    alphaValueRange.Covers(alpha))
                {
                    texturePixels[pixelIndex] = colorSwapStrategy.SwapColor(pixel);
                }
            }
        }
    }
}

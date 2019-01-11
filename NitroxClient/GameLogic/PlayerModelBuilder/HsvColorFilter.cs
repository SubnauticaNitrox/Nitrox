using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModelBuilder
{
    public class HsvColorFilter
    {
        private readonly float replacementAlpha;
        private readonly float replacementHue;
        private readonly float replacementSaturation;
        private readonly float replacementVibrancy;
        private ColorValueRange alphaValueRange;

        private ColorValueRange hueValueRange;
        private ColorValueRange saturationValueRange;
        private ColorValueRange vibrancyValueRange;

        public HsvColorFilter(float replacementHue, float replacementSaturation, float replacementVibrancy, float replacementAlpha)
        {
            this.replacementHue = replacementHue < 1f ? replacementHue : replacementHue / 360f;
            this.replacementSaturation = replacementSaturation <= 1f ? replacementSaturation : replacementSaturation / 100f;
            this.replacementVibrancy = replacementVibrancy <= 1f ? replacementVibrancy : replacementVibrancy / 100f;
            this.replacementAlpha = replacementAlpha < 1f ? replacementAlpha : replacementAlpha / 255f;

            hueValueRange = new ColorValueRange(0f, 360f);
            saturationValueRange = new ColorValueRange(0f, 100f);
            vibrancyValueRange = new ColorValueRange(0f, 100f);
            alphaValueRange = new ColorValueRange(0f, 255f);
        }

        public Color FilterColor(Color color)
        {
            float hue;
            float saturation;
            float vibrancy;
            float alpha = color.a;

            Color.RGBToHSV(color, out hue, out saturation, out vibrancy);

            if (hueValueRange.Covers(hue) &&
                saturationValueRange.Covers(saturation) &&
                vibrancyValueRange.Covers(vibrancy) &&
                alphaValueRange.Covers(alpha))
            {
                float newHue = replacementHue >= 0f ? replacementHue : hue;
                float newSaturation = replacementSaturation >= 0f ? replacementSaturation : saturation;
                float newVibrancy = replacementVibrancy >= 0f ? replacementVibrancy : vibrancy;
                float newAlpha = replacementAlpha >= 0f ? replacementAlpha : alpha;

                return Color
                    .HSVToRGB(newHue, newSaturation, newVibrancy)
                    .WithAlpha(newAlpha);
            }

            return color;
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
    }
}

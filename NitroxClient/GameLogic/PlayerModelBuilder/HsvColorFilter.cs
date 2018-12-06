using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModelBuilder
{
    public class HsvColorFilter
    {
        private readonly float replacementHue;
        private readonly float replacementSaturation;
        private readonly float replacementVibrance;
        private readonly float replacementAlpha;

        /// <summary>
        /// Constructs a HSV Color Filter object with the specified replacement values. Populate the filter ranges with the appropriate method.
        /// </summary>
        /// <param name="replacementHue">A value less than 0 will be ignored, resulting in the original hue being applied to any filtered pixels. A value greater than or equal to 1 will be divided by 360.</param>
        /// <param name="replacementSaturation">A value less than 0 will be ignored, resulting in the original saturation being applied to any filtered pixels. A value greater than 1 will be divided by 100.</param>
        /// <param name="replacementVibrance">A value less than 0 will be ignored, resulting in the original vibrance being applied to any filtered pixels. A value greater than 1 will be divided by 100.</param>
        /// <param name="replacementAlpha">A value less than 0 will be ignored, resulting in the original alpha being applied to any filtered pixels. A value greater than or equal to 1 will be divided by 255.</param>
        public HsvColorFilter(float replacementHue, float replacementSaturation, float replacementVibrance, float replacementAlpha)
        {
            HueValueRanges = new List<ColorValueRange>();
            SaturationValueRanges = new List<ColorValueRange>();
            VibranceValueRanges = new List<ColorValueRange>();
            AlphaValueRanges = new List<ColorValueRange>();

            this.replacementHue = replacementHue < 1f ? replacementHue : replacementHue / 360f;
            this.replacementSaturation = replacementSaturation <= 1f ? replacementSaturation : replacementSaturation / 100f;
            this.replacementVibrance = replacementVibrance <= 1f ? replacementVibrance : replacementVibrance / 100f;
            this.replacementAlpha = replacementAlpha < 1f ? replacementAlpha : replacementAlpha / 255f;
        }

        public List<ColorValueRange> HueValueRanges { get; }  
        public List<ColorValueRange> SaturationValueRanges { get; } 
        public List<ColorValueRange> VibranceValueRanges { get; } 
        public List<ColorValueRange> AlphaValueRanges { get; } 

        public Color FilterColor(Color color)
        {
            float hue;
            float saturation;
            float vibrance;
            float alpha = color.a;

            Color.RGBToHSV(color, out hue, out saturation, out vibrance);

            if (HueValueRanges.TrueForAll(valueRange => valueRange.Covers(hue)) &&
                SaturationValueRanges.TrueForAll(valueRange => valueRange.Covers(saturation)) &&
                VibranceValueRanges.TrueForAll(valueRange => valueRange.Covers(vibrance)) && 
                AlphaValueRanges.TrueForAll(valueRange => valueRange.Covers(alpha)))
            {
                float newHue = replacementHue >= 0f ? replacementHue : hue;
                float newSaturation = replacementSaturation >= 0f ? replacementSaturation : saturation;
                float newVibrance = replacementVibrance >= 0f ? replacementVibrance : vibrance;
                float newAplha = replacementAlpha >= 0f ? replacementAlpha : alpha;

                return Color
                    .HSVToRGB(newHue, newSaturation, newVibrance)
                    .WithAlpha(newAplha);
            }

            return color;
        }

        /// <summary>
        /// Adds a range to the hue filter that could cause pixels whose hue it covers to be replaced if their respective values are also covered by the saturation and vibrance filters.
        /// </summary>
        /// <param name="minHue">The minimum hue to be considered for this condition. Values greater than or equal to 1 will be divided by 360.</param>
        /// <param name="maxHue">The maximum hue to be considered for this condition. Values greater than 1 will be divided by 360.</param>
        public void AddHueRange(float minHue, float maxHue)
        {
            float newFilterMinValue = minHue < 1f ? minHue : minHue / 360f;
            float newFilterMaxValue = maxHue < 1f ? maxHue : maxHue / 360f;

            HueValueRanges.Add(new ColorValueRange(newFilterMinValue, newFilterMaxValue));
        }

        /// <summary>
        /// Adds a range to the saturation filter that could cause pixels whose saturation it covers to be replaced if their respective values are also covered by the hue and vibrance filters.
        /// </summary>
        /// <param name="minSaturation">The minimum saturation to be considered for this condition. Values greater than 1 will be divided by 100.</param>
        /// <param name="maxSaturation">The maximum saturation to be considered for this condition. Values greater than 1 will be divided by 100.</param>
        public void AddSaturationRange(float minSaturation, float maxSaturation)
        {
            float newFilterMinValue = minSaturation <= 1f ? minSaturation : minSaturation / 100f;
            float newFilterMaxValue = maxSaturation <= 1f ? maxSaturation : maxSaturation / 100f;

            SaturationValueRanges.Add(new ColorValueRange(newFilterMinValue, newFilterMaxValue));
        }

        /// <summary>
        /// Adds a range to the vibrance filter that could cause pixels whose vibrance it covers to be replaced if their respective values are also covered by the hue and saturation filters.
        /// </summary>
        /// <param name="minVibrance">The minimum vibrance to be considered for this condition. Values greater than 1 will be divided by 100.</param>
        /// <param name="maxVibrance">The maximum vibrance to be considered for this condition. Values greater than 1 will be divided by 100.</param>
        public void AddVibranceRange(float minVibrance, float maxVibrance)
        {
            float newFilterMinValue = minVibrance <= 1f ? minVibrance : minVibrance / 100f;
            float newFilterMaxValue = maxVibrance <= 1f ? maxVibrance : maxVibrance / 100f;

            VibranceValueRanges.Add(new ColorValueRange(newFilterMinValue, newFilterMaxValue));
        }


        /// <summary>
        /// Adds a range to the alpha filter that could cause pixels whose alpha it covers to be replaced if their respective values are also covered by the hue and saturation filters.
        /// </summary>
        /// <param name="minAlpha">The minimum alpha to be considered for this condition. Values greater than or equal to 1 will be divided by 255.</param>
        /// <param name="maxAlpha">The maximum alpha to be considered for this condition. Values greater than or equal to 1 will be divided by 255.</param>
        public void AddAlphaRange(float minAlpha, float maxAlpha)
        {
            float newFilterMinValue = minAlpha < 1f ? minAlpha : minAlpha / 255f;
            float newFilterMaxValue = maxAlpha < 1f ? maxAlpha : maxAlpha / 255f;
            
            AlphaValueRanges.Add(new ColorValueRange(newFilterMinValue, newFilterMaxValue));
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModelBuilder
{
    public class HsvColorFilter
    {
        private readonly float targetHue;
        private readonly float targetSaturation;
        private readonly float targetVibrance;

        public HsvColorFilter(Color targetColor)
        {
            Color.RGBToHSV(targetColor, out targetHue, out targetSaturation, out targetVibrance);

            HueValueRanges = new List<ColorValueRange>();
            SaturationValueRanges = new List<ColorValueRange>();
            VibranceValueRanges = new List<ColorValueRange>();
        }

        public List<ColorValueRange> HueValueRanges { get; }
        public List<ColorValueRange> SaturationValueRanges { get; }
        public List<ColorValueRange> VibranceValueRanges { get; }

        public Color FilterColor(Color color)
        {
            float hue;
            float saturation;
            float vibrance;

            Color.RGBToHSV(color, out hue, out saturation, out vibrance);

            if (HueValueRanges.TrueForAll(valueRange => valueRange.Covers(hue)) &&
                SaturationValueRanges.TrueForAll(valueRange => valueRange.Covers(saturation)) &&
                VibranceValueRanges.TrueForAll(valueRange => valueRange.Covers(vibrance)))
            {
                return Color.HSVToRGB(targetHue, saturation, vibrance);
            }

            return color;
        }

        public void AddHueRange(float minHue, float maxHue)
        {
            HueValueRanges.Add(new ColorValueRange(minHue, maxHue));
        }

        public void AddSaturationRange(float minSaturation, float maxSaturation)
        {
            SaturationValueRanges.Add(new ColorValueRange(minSaturation, maxSaturation));
        }

        public void AddVibranceRange(float minVibrance, float maxVibrance)
        {
            VibranceValueRanges.Add(new ColorValueRange(minVibrance, maxVibrance));
        }
    }
}

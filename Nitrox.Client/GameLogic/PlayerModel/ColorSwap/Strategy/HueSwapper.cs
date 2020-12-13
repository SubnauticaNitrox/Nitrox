using Nitrox.Client.GameLogic.PlayerModel.Abstract;
using UnityEngine;

namespace Nitrox.Client.GameLogic.PlayerModel.ColorSwap.Strategy
{
    public class HueSwapper : IColorSwapStrategy
    {
        private readonly float replacementHue;

        public HueSwapper(Color playerColor)
        {
            float saturation;
            float vibrancy;
            Color.RGBToHSV(playerColor, out replacementHue, out saturation, out vibrancy);
        }

        public Color SwapColor(Color originalColor)
        {
            float currentHue;
            float currentSaturation;
            float currentVibrancy;
            float currentAlpha = originalColor.a;
            Color.RGBToHSV(originalColor, out currentHue, out currentSaturation, out currentVibrancy);

            return Color
                .HSVToRGB(replacementHue, currentSaturation, currentVibrancy)
                .WithAlpha(currentAlpha);
        }
    }
}

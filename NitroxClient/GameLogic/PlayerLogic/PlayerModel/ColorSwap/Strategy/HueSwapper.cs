using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap.Strategy
{
    public class HueSwapper : IColorSwapStrategy
    {
        private readonly float replacementHue;

        public HueSwapper(Color playerColor)
        {
            Color.RGBToHSV(playerColor, out replacementHue, out float saturation, out float vibrancy);
        }

        public Color SwapColor(Color originalColor)
        {
            float currentAlpha = originalColor.a;
            Color.RGBToHSV(originalColor, out float currentHue, out float currentSaturation, out float currentVibrancy);

            return Color
                .HSVToRGB(replacementHue, currentSaturation, currentVibrancy)
                .WithAlpha(currentAlpha);
        }
    }
}

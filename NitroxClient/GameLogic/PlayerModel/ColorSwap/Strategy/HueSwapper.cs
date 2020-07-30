using NitroxClient.GameLogic.PlayerModel.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModel.ColorSwap.Strategy
{
    public class HueSwapper : IColorSwapStrategy
    {
        private readonly float replacementHue;

        public HueSwapper(Color playerColor)
        {
            Color.RGBToHSV(playerColor, out replacementHue, out float _, out float _);
        }

        public Color SwapColor(Color originalColor)
        {
            Color.RGBToHSV(originalColor, out float _, out float currentSaturation, out float currentVibrancy);

            return Color.HSVToRGB(replacementHue, currentSaturation, currentVibrancy).WithAlpha(originalColor.a);
        }
    }
}

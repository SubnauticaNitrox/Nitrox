using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap.Strategy
{
    public class HueSaturationVibrancySwapper : IColorSwapStrategy
    {
        private readonly float replacementHue;
        private readonly float replacementSaturation;
        private readonly float replacementVibrancy;

        public HueSaturationVibrancySwapper(Color playerColor)
        {
            Color.RGBToHSV(playerColor, out replacementHue, out replacementSaturation, out replacementVibrancy);
        }

        public Color SwapColor(Color originalColor)
        {
            float currentAlpha = originalColor.a;
            Color.RGBToHSV(originalColor, out float currentHue, out float currentSaturation, out float currentVibrancy);

            return Color
                .HSVToRGB(replacementHue, replacementSaturation, replacementVibrancy)
                .WithAlpha(currentAlpha);
        }
    }
}

using NitroxClient.GameLogic.PlayerModel.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModel.ColorSwap.Strategy
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
            float currentHue;
            float currentSaturation;
            float currentVibrancy;
            float currentAlpha = originalColor.a;
            Color.RGBToHSV(originalColor, out currentHue, out currentSaturation, out currentVibrancy);

            return Color
                .HSVToRGB(replacementHue, replacementSaturation, replacementVibrancy)
                .WithAlpha(currentAlpha);
        }
    }
}

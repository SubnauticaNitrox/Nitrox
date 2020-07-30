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

        public Color SwapColor(Color originalColor) => Color.HSVToRGB(replacementHue, replacementSaturation, replacementVibrancy).WithAlpha(originalColor.a);
    }
}

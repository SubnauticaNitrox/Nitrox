using Nitrox.Client.GameLogic.PlayerModel.Abstract;
using UnityEngine;

namespace Nitrox.Client.GameLogic.PlayerModel.ColorSwap.Strategy
{
    public class AlphaChannelSwapper : IColorSwapStrategy
    {
        private readonly float replacementAlpha;

        public AlphaChannelSwapper(float replacementAlpha)
        {
            this.replacementAlpha = replacementAlpha;
        }

        public Color SwapColor(Color originalColor)
        {
            return originalColor.WithAlpha(replacementAlpha);
        }
    }
}

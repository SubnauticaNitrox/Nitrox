using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap.Strategy
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

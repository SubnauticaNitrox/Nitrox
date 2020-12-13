using UnityEngine;

namespace Nitrox.Client.GameLogic.PlayerModel.Abstract
{
    public interface IColorSwapStrategy
    {
        Color SwapColor(Color originalColor);
    }
}

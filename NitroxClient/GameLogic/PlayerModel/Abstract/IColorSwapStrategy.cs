using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModel.Abstract
{
    public interface IColorSwapStrategy
    {
        Color SwapColor(Color originalColor);
    }
}

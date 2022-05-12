using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract
{
    public interface IColorSwapStrategy
    {
        Color SwapColor(Color originalColor);
    }
}

using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModel.Abstract
{
    public interface IPixelIndexer
    {
        IndexPixelsAsyncOperation IndexPixelsAsync(GameObject playerModel);
    }
}

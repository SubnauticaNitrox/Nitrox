using NitroxModel.MultiplayerSession;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModelBuilder
{
    public interface INitroxPlayer
    {
        GameObject Body { get; }
        GameObject PlayerModel { get; }
        PlayerContext PlayerContext { get; }
    }
}

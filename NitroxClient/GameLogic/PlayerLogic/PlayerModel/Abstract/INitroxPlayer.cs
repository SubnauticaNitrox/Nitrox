using NitroxModel.MultiplayerSession;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract
{
    public interface INitroxPlayer
    {
        GameObject Body { get; }
        GameObject PlayerModel { get; }
        string PlayerName { get; }
        PlayerSettings PlayerSettings { get; }
    }
}

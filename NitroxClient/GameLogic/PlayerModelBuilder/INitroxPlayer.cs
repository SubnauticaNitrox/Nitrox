using NitroxModel.MultiplayerSession;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModelBuilder
{
    public interface INitroxPlayer
    {
        GameObject Body { get; }
        GameObject PlayerModel { get; }
        string PlayerName { get; }
        PlayerSettings PlayerSettings { get; }
    }
}

using Nitrox.Model.MultiplayerSession;
using UnityEngine;

namespace Nitrox.Client.GameLogic.PlayerModel.Abstract
{
    public interface INitroxPlayer
    {
        GameObject Body { get; }
        GameObject PlayerModel { get; }
        string PlayerName { get; }
        PlayerSettings PlayerSettings { get; }
    }
}

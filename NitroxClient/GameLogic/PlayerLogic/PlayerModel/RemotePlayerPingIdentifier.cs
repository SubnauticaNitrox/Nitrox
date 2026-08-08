using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel;

/// <summary>
/// Identifies the <see cref="PingInstance"/> attached to a remote player without relying on its name or hierarchy.
/// </summary>
internal sealed class RemotePlayerPingIdentifier : MonoBehaviour
{
    internal INitroxPlayer Player { get; private set; } = null!;

    internal void Initialize(INitroxPlayer player)
    {
        Player = player;
    }
}

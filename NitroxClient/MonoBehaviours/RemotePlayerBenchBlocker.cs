using System.Collections.Generic;
using NitroxClient.GameLogic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class RemotePlayerBenchBlocker : MonoBehaviour, IConstructable
{
    private readonly List<RemotePlayer> remoteSittingPlayer = [];

    public void AddPlayerToBench(RemotePlayer remotePlayer) => remoteSittingPlayer.Add(remotePlayer);

    public void RemovePlayerFromBench(RemotePlayer remotePlayer) => remoteSittingPlayer.Remove(remotePlayer);

    public bool IsDeconstructionObstacle() => true;

    public void OnConstructedChanged(bool constructed) {}

    public bool CanDeconstruct(out string reason)
    {
        if(remoteSittingPlayer.Count != 0)
        {
            reason = Language.main.Get("Nitrox_RemotePlayerObstacle");
            return false;
        }
        reason = string.Empty;
        return true;
    }
}

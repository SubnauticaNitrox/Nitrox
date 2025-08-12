using System.Collections.Generic;
using NitroxClient.MonoBehaviours.Gui.HUD;
using UnityEngine;

namespace NitroxClient.GameLogic.HUD;

public class PlayerVitalsManager
{
    private readonly Dictionary<ushort, RemotePlayerVitals> vitalsByPlayerId = new();

    public RemotePlayerVitals CreateOrFindForPlayer(RemotePlayer remotePlayer)
    {
        if (!vitalsByPlayerId.TryGetValue(remotePlayer.PlayerId, out RemotePlayerVitals vitals))
        {
            vitalsByPlayerId[remotePlayer.PlayerId] = vitals = RemotePlayerVitals.CreateForPlayer(remotePlayer);
        }
        return vitals;
    }

    public void RemoveForPlayer(ushort playerId)
    {
        if (vitalsByPlayerId.TryGetValue(playerId, out RemotePlayerVitals vitals))
        {
            vitalsByPlayerId.Remove(playerId);
            Object.Destroy(vitals.gameObject);
        }
    }

    public bool TryFindForPlayer(ushort playerId, out RemotePlayerVitals vitals)
    {
        return vitalsByPlayerId.TryGetValue(playerId, out vitals);
    }
}

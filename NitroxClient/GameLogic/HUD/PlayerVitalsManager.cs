using System.Collections.Generic;
using Nitrox.Model.Core;
using NitroxClient.MonoBehaviours.Gui.HUD;
using UnityEngine;

namespace NitroxClient.GameLogic.HUD;

public class PlayerVitalsManager
{
    private readonly Dictionary<SessionId, RemotePlayerVitals> vitalsBySessionId = new();

    public RemotePlayerVitals CreateOrFindForPlayer(RemotePlayer remotePlayer)
    {
        if (!vitalsBySessionId.TryGetValue(remotePlayer.SessionId, out RemotePlayerVitals vitals))
        {
            vitalsBySessionId[remotePlayer.SessionId] = vitals = RemotePlayerVitals.CreateForPlayer(remotePlayer);
        }
        return vitals;
    }

    public void RemoveForPlayer(SessionId sessionId)
    {
        if (vitalsBySessionId.TryGetValue(sessionId, out RemotePlayerVitals vitals))
        {
            vitalsBySessionId.Remove(sessionId);
            Object.Destroy(vitals.gameObject);
        }
    }

    public bool TryFindForPlayer(SessionId sessionId, out RemotePlayerVitals vitals)
    {
        return vitalsBySessionId.TryGetValue(sessionId, out vitals);
    }
}

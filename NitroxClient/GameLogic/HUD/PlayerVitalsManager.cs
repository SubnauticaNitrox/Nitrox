using NitroxClient.MonoBehaviours.Gui.HUD;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.GameLogic.HUD
{
    class PlayerVitalsManager
    {
        Dictionary<string, RemotePlayerVitals> vitalsByPlayerId = new Dictionary<string, RemotePlayerVitals>();

        public void RemovePlayer(string playerId)
        {
            RemotePlayerVitals removedPlayerVitals = GetForPlayerId(playerId);
            vitalsByPlayerId.Remove(playerId);

            GameObject.Destroy(removedPlayerVitals);

            int i = 1;

            foreach (RemotePlayerVitals vitals in vitalsByPlayerId.Values)
            {
                vitals.SetNewPosition(i++);
            }
        }

        public RemotePlayerVitals GetForPlayerId(string playerId)
        {
            if (!vitalsByPlayerId.ContainsKey(playerId))
            {
                RemotePlayerVitals vitals = new GameObject().AddComponent<RemotePlayerVitals>();

                vitals.CreateVitals(playerId, vitalsByPlayerId.Count);

                vitalsByPlayerId[playerId] = vitals;
            }

            return vitalsByPlayerId[playerId];
        }
    }
}

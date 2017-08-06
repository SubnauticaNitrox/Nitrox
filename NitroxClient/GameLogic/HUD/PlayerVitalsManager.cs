using NitroxClient.MonoBehaviours.Gui.HUD;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.GameLogic.HUD
{
    class PlayerVitalsManager
    {
        Dictionary<String, RemotePlayerVitals> vitalsByPlayerId = new Dictionary<String, RemotePlayerVitals>();
        
        public void RemovePlayer(String playerId)
        {
            RemotePlayerVitals removedPlayerVitals = GetForPlayerId(playerId);
            vitalsByPlayerId.Remove(playerId);

            UnityEngine.GameObject.Destroy(removedPlayerVitals);

            int i = 1;

            foreach(RemotePlayerVitals vitals in vitalsByPlayerId.Values)
            {
                vitals.SetNewPosition(i++);
            }
        }

        public RemotePlayerVitals GetForPlayerId(String playerId)
        {
            if(!vitalsByPlayerId.ContainsKey(playerId))
            {
                RemotePlayerVitals vitals = new GameObject().AddComponent<RemotePlayerVitals>();
                vitalsByPlayerId[playerId] = vitals;

                vitals.CreateVitals(playerId, vitalsByPlayerId.Count);
            }

            return vitalsByPlayerId[playerId];
        }
    }
}

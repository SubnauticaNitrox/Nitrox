using System.Collections.Generic;
using NitroxClient.MonoBehaviours.Gui.HUD;
using UnityEngine;
using NitroxModel.Logger;
namespace NitroxClient.GameLogic.HUD
{
    public class PlayerVitalsManager
    {
        private readonly Dictionary<ushort, RemotePlayerVitals> vitalsByPlayerId = new Dictionary<ushort, RemotePlayerVitals>();

        public void RemovePlayer(ushort playerId)
        {
            RemotePlayerVitals removedPlayerVitals = GetForPlayerId(playerId, null);
            vitalsByPlayerId.Remove(playerId);

            Object.Destroy(removedPlayerVitals);
        }

        public RemotePlayerVitals GetForPlayerId(ushort playerId, PlayerManager playerManager)
        {
            RemotePlayerVitals vitals;
            if (!vitalsByPlayerId.TryGetValue(playerId, out vitals))
            {
                vitals = new GameObject().AddComponent<RemotePlayerVitals>();

                if (playerManager != null)
                {
                    vitals.CreateVitals(playerManager.Find(playerId).Get());
                }
                else
                {
                    Log.Info("PlayerManager doesn't exist are you sure this is what you want?");
                }

                vitalsByPlayerId[playerId] = vitals;
            }

            return vitals;
        }
    }
}

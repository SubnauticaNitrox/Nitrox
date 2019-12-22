using System.Collections.Generic;
using NitroxClient.MonoBehaviours.Gui.HUD;
using NitroxModel.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.HUD
{
    public class PlayerVitalsManager
    {
        private readonly Dictionary<NitroxId, RemotePlayerVitals> vitalsByPlayerId = new Dictionary<NitroxId, RemotePlayerVitals>();
        private PlayerManager PlayerManager { get; }

        public PlayerVitalsManager(PlayerManager playerManager)
        {
            PlayerManager = playerManager;
        }

        public void RemoveForPlayer(NitroxId playerId)
        {
            RemotePlayerVitals removedPlayerVitals = CreateForPlayer(playerId);
            vitalsByPlayerId.Remove(playerId);

            Object.Destroy(removedPlayerVitals);
        }

        public RemotePlayerVitals CreateForPlayer(NitroxId playerId)
        {
            RemotePlayerVitals vitals;
            if (!vitalsByPlayerId.TryGetValue(playerId, out vitals))
            {
                vitals = RemotePlayerVitals.CreateForPlayer(playerId);
                vitalsByPlayerId[playerId] = vitals;
            }

            return vitals;
        }
    }
}

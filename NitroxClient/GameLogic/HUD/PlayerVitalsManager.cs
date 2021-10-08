using System.Collections.Generic;
using NitroxClient.MonoBehaviours.Gui.HUD;
using UnityEngine;

namespace NitroxClient.GameLogic.HUD
{
    public class PlayerVitalsManager
    {
        private readonly Dictionary<ushort, RemotePlayerVitals> vitalsByPlayerId = new Dictionary<ushort, RemotePlayerVitals>();
        private PlayerManager PlayerManager { get; }

        public PlayerVitalsManager(PlayerManager playerManager)
        {
            PlayerManager = playerManager;
        }

        public void RemoveForPlayer(ushort playerId)
        {
            RemotePlayerVitals removedPlayerVitals = CreateForPlayer(playerId);
            vitalsByPlayerId.Remove(playerId);

            Object.Destroy(removedPlayerVitals);
        }

        public RemotePlayerVitals CreateForPlayer(ushort playerId)
        {
            if (!vitalsByPlayerId.TryGetValue(playerId, out RemotePlayerVitals vitals))
            {
                vitals = RemotePlayerVitals.CreateForPlayer(playerId);
                vitalsByPlayerId[playerId] = vitals;
            }

            return vitals;
        }
    }
}

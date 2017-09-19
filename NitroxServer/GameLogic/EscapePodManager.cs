using NitroxModel.DataStructures.GameLogic;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxServer.GameLogic
{
    public class EscapePodManager
    {
        private const int PLAYERS_PER_ESCAPEPOD = 50;
        private const int ESCAPE_POD_X_OFFSET = 40;

        private List<EscapePodModel> escapePods;
        private Dictionary<String, EscapePodModel> escapePodsByPlayerId;
        private EscapePodModel podNotFullYet;

        public EscapePodManager()
        {
            this.escapePods = new List<EscapePodModel>();
            this.escapePodsByPlayerId = new Dictionary<String, EscapePodModel>();
            this.podNotFullYet = CreateNewEscapePod();
        }

        public void AssignPlayerToEscapePod(String playerId)
        {
            lock(escapePodsByPlayerId)
            {
                if(escapePodsByPlayerId.ContainsKey(playerId))
                {
                    return;
                }

                if (podNotFullYet.AssignedPlayers.Count == PLAYERS_PER_ESCAPEPOD)
                {
                    podNotFullYet = CreateNewEscapePod();
                }

                podNotFullYet.AssignedPlayers.Add(playerId);
                escapePodsByPlayerId[playerId] = podNotFullYet;
            }
        }

        public EscapePodModel[] GetEscapePods()
        {
            lock (escapePods)
            {
                return escapePods.ToArray();
            }
        }

        private EscapePodModel CreateNewEscapePod()
        {
            lock (escapePods)
            {
                int totalEscapePods = escapePods.Count; 

                EscapePodModel escapePod = new EscapePodModel("escapePod" + totalEscapePods, 
                                                              new Vector3(-112.2f + (ESCAPE_POD_X_OFFSET * totalEscapePods), 0.0f, -322.6f),
                                                              "escapePodFab" + totalEscapePods,
                                                              "escapePodMedFab" + totalEscapePods,
                                                              "escapePodStorageFab" + totalEscapePods,
                                                              "escapePodRadioFab" + totalEscapePods);               
                escapePods.Add(escapePod);

                return escapePod;
            }
        }
    }
}

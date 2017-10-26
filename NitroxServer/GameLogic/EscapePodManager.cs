using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using UnityEngine;

namespace NitroxServer.GameLogic
{
    public class EscapePodManager
    {
        private const int PLAYERS_PER_ESCAPEPOD = 50;
        private const int ESCAPE_POD_X_OFFSET = 40;

        private readonly List<EscapePodModel> escapePods = new List<EscapePodModel>();
        private readonly Dictionary<string, EscapePodModel> escapePodsByPlayerId = new Dictionary<string, EscapePodModel>();
        private EscapePodModel podNotFullYet;

        public EscapePodManager()
        {
            podNotFullYet = CreateNewEscapePod();
        }

        public void AssignPlayerToEscapePod(string playerId)
        {
            lock (escapePodsByPlayerId)
            {
                if (escapePodsByPlayerId.ContainsKey(playerId))
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

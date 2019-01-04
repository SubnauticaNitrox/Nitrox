using NitroxModel.DataStructures.GameLogic;
using UnityEngine;

namespace NitroxServer.GameLogic
{
    public class EscapePodManager
    {
        public const int PLAYERS_PER_ESCAPEPOD = 50;

        public const int ESCAPE_POD_X_OFFSET = 40;

        private readonly EscapePodData escapePodData;

        public EscapePodManager(EscapePodData escapePodData)
        {
            this.escapePodData = escapePodData;

            if (escapePodData.escapePods.Count > 0)
                escapePodData.podNotFullYet = escapePodData.escapePods.GetLast();

            if (escapePodData.podNotFullYet == null)
                escapePodData.podNotFullYet = CreateNewEscapePod();
        }

        public void AssignPlayerToEscapePod(ushort playerId)
        {
            lock (escapePodData.escapePodsByPlayerId)
            {
                if (escapePodData.escapePodsByPlayerId.ContainsKey(playerId))
                {
                    return;
                }

                if (escapePodData.podNotFullYet.AssignedPlayers.Count == PLAYERS_PER_ESCAPEPOD)
                {
                    escapePodData.podNotFullYet = CreateNewEscapePod();
                }

                escapePodData.podNotFullYet.AssignedPlayers.Add(playerId);
                escapePodData.escapePodsByPlayerId[playerId] = escapePodData.podNotFullYet;
            }
        }

        public EscapePodModel[] GetEscapePods()
        {
            lock (escapePodData.escapePods)
            {
                return escapePodData.escapePods.ToArray();
            }
        }

        private EscapePodModel CreateNewEscapePod()
        {
            lock (escapePodData.escapePods)
            {
                int totalEscapePods = escapePodData.escapePods.Count;

                EscapePodModel escapePod = new EscapePodModel();
                escapePod.InitEscapePodModel("escapePod" + totalEscapePods,
                    new Vector3(-112.2f + ESCAPE_POD_X_OFFSET * totalEscapePods, 0.0f, -322.6f),
                    "escapePodFab" + totalEscapePods,
                    "escapePodMedFab" + totalEscapePods,
                    "escapePodStorageFab" + totalEscapePods,
                    "escapePodRadioFab" + totalEscapePods);

                escapePodData.escapePods.Add(escapePod);

                return escapePod;
            }
        }
    }
}

using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
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


            if (escapePodData.EscapePods.Count > 0)
                escapePodData.PodNotFullYet = escapePodData.EscapePods.GetLast();

            if (escapePodData.PodNotFullYet == null)
                escapePodData.PodNotFullYet = CreateNewEscapePod();
        }

        public Optional<EscapePodModel> AssignPlayerToEscapePod(ushort playerId)
        {
            Optional<EscapePodModel> escapePod = Optional<EscapePodModel>.Empty();
            lock (escapePodData.EscapePodsByPlayerId)
            {
                if (escapePodData.EscapePodsByPlayerId.ContainsKey(playerId))
                {
                    return escapePod;
                }

                if (escapePodData.PodNotFullYet.AssignedPlayers.Count == PLAYERS_PER_ESCAPEPOD)
                {
                    escapePod = Optional<EscapePodModel>.Of(CreateNewEscapePod());
                    escapePodData.PodNotFullYet = escapePod.Get();
                }

                escapePodData.PodNotFullYet.AssignedPlayers.Add(playerId);
                escapePodData.EscapePodsByPlayerId[playerId] = escapePodData.PodNotFullYet;
            }
            return escapePod;
        }

        private EscapePodModel CreateNewEscapePod()
        {
            lock (escapePodData.EscapePods)
            {
                int totalEscapePods = escapePodData.EscapePods.Count;

                EscapePodModel escapePod = new EscapePodModel();
                escapePod.InitEscapePodModel("escapePod" + totalEscapePods,
                    new Vector3(-112.2f + ESCAPE_POD_X_OFFSET * totalEscapePods, 0.0f, -322.6f),
                    "escapePodFab" + totalEscapePods,
                    "escapePodMedFab" + totalEscapePods,
                    "escapePodStorageFab" + totalEscapePods,
                    "escapePodRadioFab" + totalEscapePods);

                escapePodData.EscapePods.Add(escapePod);


                return escapePod;
            }
        }
    }
}

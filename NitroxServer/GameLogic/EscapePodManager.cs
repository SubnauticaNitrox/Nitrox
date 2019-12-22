using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
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
                escapePodData.PodNotFullYet = escapePodData.EscapePods[escapePodData.EscapePods.Count - 1];

            if (escapePodData.PodNotFullYet == null)
                escapePodData.PodNotFullYet = CreateNewEscapePod();
        }

        public NitroxId AssignPlayerToEscapePod(NitroxId playerId, out Optional<EscapePodModel> newlyCreatedPod)
        {
            newlyCreatedPod = Optional<EscapePodModel>.Empty();
            lock (escapePodData.EscapePodsByPlayerId)
            {
                if (escapePodData.EscapePodsByPlayerId.ContainsKey(playerId))
                {
                    return escapePodData.EscapePodsByPlayerId[playerId].Id;
                }

                if (escapePodData.PodNotFullYet.AssignedPlayers.Count == PLAYERS_PER_ESCAPEPOD)
                {
                    newlyCreatedPod = Optional<EscapePodModel>.Of(CreateNewEscapePod());
                    escapePodData.PodNotFullYet = newlyCreatedPod.Get();
                }

                escapePodData.PodNotFullYet.AssignedPlayers.Add(playerId);
                escapePodData.EscapePodsByPlayerId[playerId] = escapePodData.PodNotFullYet;
            }
            return escapePodData.PodNotFullYet.Id;
        }

        private EscapePodModel CreateNewEscapePod()
        {
            lock (escapePodData.EscapePods)
            {
                int totalEscapePods = escapePodData.EscapePods.Count;

                EscapePodModel escapePod = new EscapePodModel();
                escapePod.InitEscapePodModel(new NitroxId(),
                    new Vector3(-112.2f + ESCAPE_POD_X_OFFSET * totalEscapePods, 0.0f, -322.6f),
                    new NitroxId(),
                    new NitroxId(),
                    new NitroxId(),
                    new NitroxId(),
                    true,
                    true);

                escapePodData.EscapePods.Add(escapePod);

                return escapePod;
            }
        }

        public void RepairEscapePod(NitroxId id)
        {
            lock(escapePodData.EscapePods)
            {
                EscapePodModel escapePod = escapePodData.EscapePods.Find(ep => ep.Id == id);
                escapePod.Damaged = false;
            }
        }

        public void RepairEscapePodRadio(NitroxId id)
        {
            lock (escapePodData.EscapePods)
            {
                EscapePodModel escapePod = escapePodData.EscapePods.Find(ep => ep.RadioId == id);
                escapePod.RadioDamaged = false;
            }
        }
    }
}

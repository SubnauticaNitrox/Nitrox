using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using UnityEngine;
using System.Collections.Generic;

namespace NitroxServer.GameLogic
{
    public class EscapePodManager
    {
        public const int ESCAPE_POD_X_OFFSET = 40;

        private List<EscapePodModel> escapePods;
        private Dictionary<ushort, EscapePodModel> escapePodsByPlayerId;
        private EscapePodModel podForNextPlayer;

        public EscapePodManager(List<EscapePodModel> escapePods)
        {
            this.escapePods = escapePods;

            initializePodForNextPlayer();
            initializeEscapePodsByPlayerId();
        }

        public NitroxId AssignPlayerToEscapePod(ushort playerId, out Optional<EscapePodModel> newlyCreatedPod)
        {
            newlyCreatedPod = Optional<EscapePodModel>.Empty();

            lock (escapePodsByPlayerId)
            {
                if (escapePodsByPlayerId.ContainsKey(playerId))
                {
                    return escapePodsByPlayerId[playerId].Id;
                }

                if (podForNextPlayer.IsFull())
                {
                    newlyCreatedPod = Optional<EscapePodModel>.Of(CreateNewEscapePod());
                    podForNextPlayer = newlyCreatedPod.Get();
                }

                podForNextPlayer.AssignedPlayers.Add(playerId);
                escapePodsByPlayerId[playerId] = podForNextPlayer;
            }

            return podForNextPlayer.Id;
        }

        public List<EscapePodModel> GetEscapePods()
        {
            lock(escapePods)
            {
                // coyp to prevent mutation at the caller
                return new List<EscapePodModel>(escapePods);
            }
        }

        public void RepairEscapePod(NitroxId id)
        {
            lock(escapePods)
            {
                EscapePodModel escapePod = escapePods.Find(ep => ep.Id == id);
                escapePod.Damaged = false;
            }
        }

        public void RepairEscapePodRadio(NitroxId id)
        {
            lock (escapePods)
            {
                EscapePodModel escapePod = escapePods.Find(ep => ep.RadioId == id);
                escapePod.RadioDamaged = false;
            }
        }
        
        private EscapePodModel CreateNewEscapePod()
        {
            lock (escapePods)
            {
                int totalEscapePods = escapePods.Count;

                EscapePodModel escapePod = new EscapePodModel();
                escapePod.InitEscapePodModel(new NitroxId(),
                    new Vector3(-112.2f + ESCAPE_POD_X_OFFSET * totalEscapePods, 0.0f, -322.6f),
                    new NitroxId(),
                    new NitroxId(),
                    new NitroxId(),
                    new NitroxId(),
                    true,
                    true);

                escapePods.Add(escapePod);

                return escapePod;
            }
        }

        private void initializePodForNextPlayer()
        {
            foreach (EscapePodModel pod in escapePods)
            {
                if (!pod.IsFull())
                {
                    podForNextPlayer = pod;
                    return;
                }
            }

            podForNextPlayer = CreateNewEscapePod();
        }

        private void initializeEscapePodsByPlayerId()
        {
            escapePodsByPlayerId = new Dictionary<ushort, EscapePodModel>();

            foreach (EscapePodModel pod in escapePods)
            {
                foreach (ushort playerId in pod.AssignedPlayers)
                {
                    escapePodsByPlayerId[playerId] = pod;
                }
            }
        }
    }
}

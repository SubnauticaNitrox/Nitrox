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

        public ThreadSafeCollection<EscapePodModel> EscapePods { get; }
        private readonly ThreadSafeDictionary<ushort, EscapePodModel> escapePodsByPlayerId = new ThreadSafeDictionary<ushort, EscapePodModel>();
        private EscapePodModel podForNextPlayer;

        public EscapePodManager(List<EscapePodModel> escapePods)
        {
            EscapePods = new ThreadSafeCollection<EscapePodModel>(escapePods);

            InitializePodForNextPlayer();
            InitializeEscapePodsByPlayerId();
        }

        public NitroxId AssignPlayerToEscapePod(ushort playerId, out Optional<EscapePodModel> newlyCreatedPod)
        {
            newlyCreatedPod = Optional.Empty;

            if (escapePodsByPlayerId.ContainsKey(playerId))
            {
                return escapePodsByPlayerId[playerId].Id;
            }

            if (podForNextPlayer.IsFull())
            {
                newlyCreatedPod = Optional.Of(CreateNewEscapePod());
                podForNextPlayer = newlyCreatedPod.Value;
            }

            podForNextPlayer.AssignedPlayers.Add(playerId);
            escapePodsByPlayerId[playerId] = podForNextPlayer;

            return podForNextPlayer.Id;
        }

        public List<EscapePodModel> GetEscapePods()
        {
            return EscapePods.ToList();
        }

        public void RepairEscapePod(NitroxId id)
        {
            EscapePodModel escapePod = EscapePods.Find(ep => ep.Id == id);
            escapePod.Damaged = false;
        }

        public void RepairEscapePodRadio(NitroxId id)
        {
            EscapePodModel escapePod = EscapePods.Find(ep => ep.RadioId == id);
            escapePod.RadioDamaged = false;
        }
        
        private EscapePodModel CreateNewEscapePod()
        {
            int totalEscapePods = EscapePods.Count;

            EscapePodModel escapePod = new EscapePodModel();
            escapePod.InitEscapePodModel(new NitroxId(),
                                         new Vector3(-112.2f + ESCAPE_POD_X_OFFSET * totalEscapePods, 0.0f, -322.6f),
                                         new NitroxId(),
                                         new NitroxId(),
                                         new NitroxId(),
                                         new NitroxId(),
                                         true,
                                         true);

            EscapePods.Add(escapePod);

            return escapePod;
        }

        private void InitializePodForNextPlayer()
        {
            foreach (EscapePodModel pod in EscapePods)
            {
                if (!pod.IsFull())
                {
                    podForNextPlayer = pod;
                    return;
                }
            }

            podForNextPlayer = CreateNewEscapePod();
        }

        private void InitializeEscapePodsByPlayerId()
        {
            escapePodsByPlayerId.Clear();
            foreach (EscapePodModel pod in EscapePods)
            {
                foreach (ushort playerId in pod.AssignedPlayers)
                {
                    escapePodsByPlayerId[playerId] = pod;
                }
            }
        }
    }
}

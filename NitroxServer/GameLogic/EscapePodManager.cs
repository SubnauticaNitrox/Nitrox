using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;

namespace NitroxServer.GameLogic
{
    public class EscapePodManager
    {
        public const int ESCAPE_POD_X_OFFSET = 40;

        public ThreadSafeList<EscapePodModel> EscapePods { get; }
        private readonly ThreadSafeDictionary<ushort, EscapePodModel> escapePodsByPlayerId = new ThreadSafeDictionary<ushort, EscapePodModel>();
        private EscapePodModel podForNextPlayer;
        private readonly string seed;

        private readonly RandomStartGenerator randomStart;

        public EscapePodManager(List<EscapePodModel> escapePods, RandomStartGenerator randomStart, string seed)
        {
            EscapePods = new ThreadSafeList<EscapePodModel>(escapePods);

            this.seed = seed;
            this.randomStart = randomStart;

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
            EscapePodModel escapePod = new EscapePodModel();
            escapePod.InitEscapePodModel(new NitroxId(),
                                         GetStartPosition(),
                                         new NitroxId(),
                                         new NitroxId(),
                                         new NitroxId(),
                                         new NitroxId(),
                                         true,
                                         true);

            EscapePods.Add(escapePod);

            return escapePod;
        }

        private NitroxVector3 GetStartPosition()
        {
            Random rnd = new Random(seed.GetHashCode());
            NitroxVector3 position = randomStart.GenerateRandomStartPosition(rnd);

            if (EscapePods.Count == 0)
            {
                return position;
            }

            foreach (EscapePodModel escapePodModel in EscapePods)
            {
                if (position == NitroxVector3.Zero)
                {
                    break;
                }

                if (escapePodModel.Location != position)
                {
                    return position;
                }
            }

            float xNormed = (float)rnd.NextDouble();
            float zNormed = (float)rnd.NextDouble();

            if (xNormed < 0.3f)
            {
                xNormed = 0.3f;
            }
            else if (xNormed > 0.7f)
            {
                xNormed = 0.7f;
            }

            if (zNormed < 0.3f)
            {
                zNormed = 0.3f;
            }
            else if (zNormed > 0.7f)
            {
                zNormed = 0.7f;
            }

            NitroxVector3 lastEscapePodPosition = EscapePods[EscapePods.Count - 1].Location;

            float x = xNormed * 100 - 50;
            float z = zNormed * 100 - 50;

            return new NitroxVector3(lastEscapePodPosition.X + x, 0, lastEscapePodPosition.Z + z);
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

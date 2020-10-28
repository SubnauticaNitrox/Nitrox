using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using System.Collections.Generic;
using NitroxModel.Logger;

namespace NitroxServer.GameLogic
{
    public class EscapePodManager
    {
        public const int ESCAPE_POD_X_OFFSET = 40;
        private int[,,] ESCAPE_POD_SPAWN_LOCATIONS = new int[,,] {
            { { -200, -112 }, { -193, -9 } },
            { { -192, -112 }, { -185, 103 } },
            { { -184, -120 }, { -177, 111 } },
            { { -176, -128 }, { -169, 135 } },
            { { -168, -296 }, { -161, 167 } },
            { { -160, -400 }, { -153, 175 } },
            { { -152, -432 }, { -145, 183 } },
            { { -144, -448 }, { -137, 175 } },
            { { -136, -448 }, { -129, 175 } },
            { { -128, -424 }, { -121, 167 } },
            { { -120, -408 }, { -113, 167 } },
            { { -112, -400 }, { -105, 167 } },
            { { -104, -400 }, { -100, 159 } },
            { { -99, -400 }, { -97, 159 } },
            { { -96, -392 }, { -89, 159 } },
            { { -88, -392 }, { -81, 159 } },
            { { -80, -392 }, { -73, 151 } },
            { { -72, -400 }, { -65, 143 } },
            { { -64, -400 }, { -57, 135 } },
            { { -56, -408 }, { -49, 135 } },
            { { -48, -400 }, { -41, 135 } },
            { { -40, -384 }, { -33, 311 } },
            { { -32, -376 }, { -17, 303 } },
            { { -16, -360 }, { -10, 303 } },
            { { -9, -360 }, { -9, 303 } },
            { { -8, -352 }, { -1, 303 } },
            { { 0, -344 }, { 7, 303 } },
            { { 8, -344 }, { 9, 303 } },
            { { 10, -344 }, { 15, 303 } },
            { { 16, -336 }, { 23, 303 } },
            { { 24, -328 }, { 31, 231 } },
            { { 32, -328 }, { 39, 215 } },
            { { 40, -320 }, { 47, 207 } },
            { { 48, -320 }, { 55, 199 } },
            { { 56, -320 }, { 63, 191 } },
            { { 64, -320 }, { 71, 183 } },
            { { 72, -24 }, { 79, 119 } },
            { { 80, 24 }, { 87, 79 } }};

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
            EscapePodModel escapePod = new EscapePodModel();
            escapePod.InitEscapePodModel(new NitroxId(),
                                         GenerateSpawnLocation(),
                                         new NitroxId(),
                                         new NitroxId(),
                                         new NitroxId(),
                                         new NitroxId(),
                                         true,
                                         true);

            EscapePods.Add(escapePod);

            return escapePod;
        }

        private NitroxVector3 GenerateSpawnLocation()
        {
            int totalEscapePods = EscapePods.Count;
            if (totalEscapePods > 0)
            {
                NitroxVector3 originalLocation = EscapePods[0].Location;
                var b = new NitroxVector3(originalLocation.X + (ESCAPE_POD_X_OFFSET * totalEscapePods), 0.0f, originalLocation.Z);
                Log.Debug($"Next Escape pod is {b}");
                return b;
            }

            Random r = new Random();
            int randomLocationSelector = r.Next(0, 37);
            Log.Debug($"Random spawn array index is {randomLocationSelector}");
            int xUpper = (int)ESCAPE_POD_SPAWN_LOCATIONS.GetValue(randomLocationSelector, 0, 0);
            Log.Debug($"xUpper : {xUpper}");
            int xLower = (int)ESCAPE_POD_SPAWN_LOCATIONS.GetValue(randomLocationSelector, 1, 0);
            Log.Debug($"xLower : {xLower}");
            int zUpper = (int)ESCAPE_POD_SPAWN_LOCATIONS.GetValue(randomLocationSelector, 0, 1);
            Log.Debug($"zUpper : {zUpper}");
            int zLower = (int)ESCAPE_POD_SPAWN_LOCATIONS.GetValue(randomLocationSelector, 1, 1);
            Log.Debug($"zLower : {zLower}");
            int x = r.Next(xUpper, xLower);
            Log.Debug($"x random {x}");
            int z = r.Next(zUpper, zLower);
            Log.Debug($"z random {z}");
            Log.Debug($"New random is {new NitroxVector3(x, 0, z)}");
            return new NitroxVector3(x, 0, z);
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

using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.GameLogic
{
    public class EscapePodManager
    {
        public const int PLAYERS_PER_ESCAPEPOD = 50;
        public const int ESCAPE_POD_X_OFFSET = 40;

        private readonly EntityRegistry entityRegistry;
        private readonly ThreadSafeDictionary<ushort, EscapePodWorldEntity> escapePodsByPlayerId = new ThreadSafeDictionary<ushort, EscapePodWorldEntity>();
        private EscapePodWorldEntity podForNextPlayer;
        private readonly string seed;

        private readonly RandomStartGenerator randomStart;

        public EscapePodManager(EntityRegistry entityRegistry, RandomStartGenerator randomStart, string seed)
        {
            this.seed = seed;
            this.randomStart = randomStart;
            this.entityRegistry = entityRegistry;

            List<EscapePodWorldEntity> escapePods = entityRegistry.GetEntities<EscapePodWorldEntity>();

            InitializePodForNextPlayer(escapePods);
            InitializeEscapePodsByPlayerId(escapePods);
        }

        public NitroxId AssignPlayerToEscapePod(ushort playerId, out Optional<EscapePodWorldEntity> newlyCreatedPod)
        {
            newlyCreatedPod = Optional.Empty;

            if (escapePodsByPlayerId.ContainsKey(playerId))
            {
                return escapePodsByPlayerId[playerId].Id;
            }

            if (IsPodFull(podForNextPlayer))
            {
                newlyCreatedPod = Optional.Of(CreateNewEscapePod());
                podForNextPlayer = newlyCreatedPod.Value;
            }

            podForNextPlayer.Players.Add(playerId);
            escapePodsByPlayerId[playerId] = podForNextPlayer;

            return podForNextPlayer.Id;
        }

        private EscapePodWorldEntity CreateNewEscapePod()
        {
            EscapePodWorldEntity escapePod = new EscapePodWorldEntity(GetStartPosition(), new NitroxId(), null);

            escapePod.ChildEntities.Add(new PrefabChildEntity(new NitroxId(), "5c06baec-0539-4f26-817d-78443548cc52", new NitroxTechType("Radio"), 0, null, escapePod.Id));
            escapePod.ChildEntities.Add(new PrefabChildEntity(new NitroxId(), "c0175cf7-0b6a-4a1d-938f-dad0dbb6fa06", new NitroxTechType("MedicalCabinet"), 0, null, escapePod.Id));
            escapePod.ChildEntities.Add(new PrefabChildEntity(new NitroxId(), "9f16d82b-11f4-4eeb-aedf-f2fa2bfca8e3", new NitroxTechType("Fabricator"), 0, null, escapePod.Id));
            escapePod.ChildEntities.Add(new InventoryEntity(0, new NitroxId(), new NitroxTechType("SmallStorage"), null, escapePod.Id, new List<Entity>()));

            entityRegistry.AddEntity(escapePod);
            entityRegistry.AddEntities(escapePod.ChildEntities);

            return escapePod;
        }

        private NitroxVector3 GetStartPosition()
        {
            List<EscapePodWorldEntity> escapePods = entityRegistry.GetEntities<EscapePodWorldEntity>();

            Random rnd = new Random(seed.GetHashCode());
            NitroxVector3 position = randomStart.GenerateRandomStartPosition(rnd);

            if (escapePods.Count == 0)
            {
                return position;
            }

            foreach (EscapePodWorldEntity escapePodModel in escapePods)
            {
                if (position == NitroxVector3.Zero)
                {
                    break;
                }

                if (escapePodModel.Transform.Position != position)
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

            NitroxVector3 lastEscapePodPosition = escapePods[escapePods.Count - 1].Transform.Position;

            float x = xNormed * 100 - 50;
            float z = zNormed * 100 - 50;

            return new NitroxVector3(lastEscapePodPosition.X + x, 0, lastEscapePodPosition.Z + z);
        }

        private void InitializePodForNextPlayer(List<EscapePodWorldEntity> escapePods)
        {
            foreach (EscapePodWorldEntity pod in escapePods)
            {
                if (!IsPodFull(pod))
                {
                    podForNextPlayer = pod;
                    return;
                }
            }

            podForNextPlayer = CreateNewEscapePod();
        }

        private void InitializeEscapePodsByPlayerId(List<EscapePodWorldEntity> escapePods)
        {
            escapePodsByPlayerId.Clear();
            foreach (EscapePodWorldEntity pod in escapePods)
            {
                foreach (ushort playerId in pod.Players)
                {
                    escapePodsByPlayerId[playerId] = pod;
                }
            }
        }

        private static bool IsPodFull(EscapePodWorldEntity pod)
        {
            return pod.Players.Count >= PLAYERS_PER_ESCAPEPOD;
        }
    }
}

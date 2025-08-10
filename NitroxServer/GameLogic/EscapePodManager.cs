using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.GameLogic;

public class EscapePodManager
{
    private const int PLAYERS_PER_ESCAPEPOD = 50;

    private readonly EntityRegistry entityRegistry;
    private readonly ThreadSafeDictionary<ushort, EscapePodEntity> escapePodsByPlayerId = new();
    private EscapePodEntity podForNextPlayer;
    private readonly string seed;

    private readonly RandomStartGenerator randomStart;

    public EscapePodManager(EntityRegistry entityRegistry, RandomStartGenerator randomStart, string seed)
    {
        this.seed = seed;
        this.randomStart = randomStart;
        this.entityRegistry = entityRegistry;

        List<EscapePodEntity> escapePods = entityRegistry.GetEntities<EscapePodEntity>();

        InitializePodForNextPlayer(escapePods);
        InitializeEscapePodsByPlayerId(escapePods);
    }

    public NitroxId AssignPlayerToEscapePod(ushort playerId, out Optional<EscapePodEntity> newlyCreatedPod)
    {
        newlyCreatedPod = Optional.Empty;

        if (escapePodsByPlayerId.TryGetValue(playerId, out EscapePodEntity podEntity))
        {
            return podEntity.Id;
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

    private EscapePodEntity CreateNewEscapePod()
    {
        EscapePodEntity escapePod = new(GetStartPosition(), new NitroxId(), new EscapePodMetadata(false, false));

        escapePod.ChildEntities.Add(new PrefabChildEntity(new NitroxId(), "5c06baec-0539-4f26-817d-78443548cc52", new NitroxTechType("Radio"), 0, null, escapePod.Id));
        escapePod.ChildEntities.Add(new PrefabChildEntity(new NitroxId(), "c0175cf7-0b6a-4a1d-938f-dad0dbb6fa06", new NitroxTechType("MedicalCabinet"), 0, null, escapePod.Id));
        escapePod.ChildEntities.Add(new PrefabChildEntity(new NitroxId(), "9f16d82b-11f4-4eeb-aedf-f2fa2bfca8e3", new NitroxTechType("Fabricator"), 0, null, escapePod.Id));
        escapePod.ChildEntities.Add(new InventoryEntity(0, new NitroxId(), new NitroxTechType("SmallStorage"), null, escapePod.Id, []));

        entityRegistry.AddOrUpdate(escapePod);

        return escapePod;
    }

    private NitroxVector3 GetStartPosition()
    {
        List<EscapePodEntity> escapePods = entityRegistry.GetEntities<EscapePodEntity>();

        Random rnd = new(seed.GetHashCode());
        NitroxVector3 position = randomStart.GenerateRandomStartPosition(rnd);

        if (escapePods.Count == 0)
        {
            return position;
        }

        foreach (EscapePodEntity escapePodModel in escapePods)
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

    private void InitializePodForNextPlayer(List<EscapePodEntity> escapePods)
    {
        foreach (EscapePodEntity pod in escapePods)
        {
            if (!IsPodFull(pod))
            {
                podForNextPlayer = pod;
                return;
            }
        }

        podForNextPlayer = CreateNewEscapePod();
    }

    private void InitializeEscapePodsByPlayerId(List<EscapePodEntity> escapePods)
    {
        escapePodsByPlayerId.Clear();
        foreach (EscapePodEntity pod in escapePods)
        {
            foreach (ushort playerId in pod.Players)
            {
                escapePodsByPlayerId[playerId] = pod;
            }
        }
    }

    private static bool IsPodFull(EscapePodEntity pod)
    {
        return pod.Players.Count >= PLAYERS_PER_ESCAPEPOD;
    }
}

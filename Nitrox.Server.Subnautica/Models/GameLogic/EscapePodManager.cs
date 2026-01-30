using System.Collections.Generic;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

internal class EscapePodManager(EntityRegistry entityRegistry, RandomStartResource randomStartResource, IOptions<SubnauticaServerOptions> options)
{
    private const int PLAYERS_PER_ESCAPEPOD = 50;

    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly RandomStartResource randomStartResource = randomStartResource;
    private readonly IOptions<SubnauticaServerOptions> options = options;
    private readonly ThreadSafeDictionary<PeerId, EscapePodEntity> escapePodsByPlayerId = [];
    private EscapePodEntity? podForNextPlayer;

    public NitroxId AssignPlayerToEscapePod(PeerId playerId, out Optional<EscapePodEntity> newlyCreatedPod)
    {
        newlyCreatedPod = Optional.Empty;
        if (escapePodsByPlayerId.TryGetValue(playerId, out EscapePodEntity podEntity))
        {
            return podEntity.Id;
        }

        if (podForNextPlayer == null || IsPodFull(podForNextPlayer))
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

        string seed = options.Value.Seed;
        if (string.IsNullOrWhiteSpace(seed))
        {
            throw new InvalidOperationException();
        }
        Random rnd = new(seed.GetHashCode());
        NitroxVector3 position = randomStartResource.RandomStartGenerator.GenerateRandomStartPosition(rnd);

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

        NitroxVector3 lastEscapePodPosition = escapePods[^1].Transform.Position;

        float x = xNormed * 100 - 50;
        float z = zNormed * 100 - 50;

        return new NitroxVector3(lastEscapePodPosition.X + x, 0, lastEscapePodPosition.Z + z);
    }

    public void AddKnownPods(IReadOnlyCollection<EscapePodEntity> escapePods)
    {
        InitializePodForNextPlayer();
        InitializeEscapePodsByPlayerId();

        void InitializePodForNextPlayer()
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

        void InitializeEscapePodsByPlayerId()
        {
            foreach (EscapePodEntity pod in escapePods)
            {
                foreach (PeerId playerId in pod.Players)
                {
                    escapePodsByPlayerId[playerId] = pod;
                }
            }
        }
    }

    private static bool IsPodFull(EscapePodEntity pod)
    {
        return pod.Players.Count >= PLAYERS_PER_ESCAPEPOD;
    }
}

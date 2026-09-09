using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Server.Subnautica.Models.Factories;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

internal class EscapePodManager(RandomFactory randomFactory, EntityRegistry entityRegistry, RandomStartResource randomStartResource, IOptions<SubnauticaServerOptions> options, IPacketSender packetSender)
{
    private const int PLAYERS_PER_ESCAPEPOD = 50;

    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly RandomStartResource randomStartResource = randomStartResource;
    private readonly IOptions<SubnauticaServerOptions> options = options;
    private readonly IPacketSender packetSender = packetSender;
    private readonly ThreadSafeDictionary<PeerId, EscapePodEntity> escapePodsByPlayerId = [];
    private EscapePodEntity? podForNextPlayer;
    private readonly Random random = randomFactory.GetDotnetRandom();

    public async Task<(NitroxId escapePodId, EscapePodEntity? newlyCreatedPod)> AssignPlayerToEscapePodAsync(PeerId playerId)
    {
        if (escapePodsByPlayerId.TryGetValue(playerId, out EscapePodEntity podEntity))
        {
            return (podEntity.Id, null);
        }

        if (!HasEmptySlot(podForNextPlayer))
        {
            podForNextPlayer = await CreateNewEscapePodAsync();
        }

        podForNextPlayer.Players.Add(playerId);
        escapePodsByPlayerId[playerId] = podForNextPlayer;

        return (podForNextPlayer.Id, podForNextPlayer);
    }

    private async Task<EscapePodEntity> CreateNewEscapePodAsync()
    {
        EscapePodEntity escapePod = new(await GetStartPositionAsync(), new NitroxId(), new EscapePodMetadata(false, false));

        escapePod.ChildEntities.Add(new PrefabChildEntity(new NitroxId(), "5c06baec-0539-4f26-817d-78443548cc52", new NitroxTechType("Radio"), 0, null, escapePod.Id));
        escapePod.ChildEntities.Add(new PrefabChildEntity(new NitroxId(), "c0175cf7-0b6a-4a1d-938f-dad0dbb6fa06", new NitroxTechType("MedicalCabinet"), 0, null, escapePod.Id));
        escapePod.ChildEntities.Add(new PrefabChildEntity(new NitroxId(), "9f16d82b-11f4-4eeb-aedf-f2fa2bfca8e3", new NitroxTechType("Fabricator"), 0, null, escapePod.Id));
        escapePod.ChildEntities.Add(new InventoryEntity(0, new NitroxId(), new NitroxTechType("SmallStorage"), null, escapePod.Id, []));

        entityRegistry.AddOrUpdate(escapePod);

        return escapePod;
    }

    private async Task<NitroxVector3> GetStartPositionAsync()
    {
        List<EscapePodEntity> escapePods = entityRegistry.GetEntities<EscapePodEntity>();

        string seed = options.Value.Seed;
        if (string.IsNullOrWhiteSpace(seed))
        {
            throw new InvalidOperationException();
        }
        RandomStartGenerator randomStartGenerator = await randomStartResource.GetRandomStartGeneratorAsync();
        NitroxVector3 position = randomStartGenerator.GenerateAllStartPositions(random).FirstOrDefault();

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

        float xNormed = (float)random.NextDouble();
        float zNormed = (float)random.NextDouble();

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

    public async Task AddKnownPodsAsync(IReadOnlyCollection<EscapePodEntity> escapePods)
    {
        await InitializePodForNextPlayerAsync();
        InitializeEscapePodsByPlayerId();

        async Task InitializePodForNextPlayerAsync()
        {
            foreach (EscapePodEntity pod in escapePods)
            {
                if (HasEmptySlot(pod))
                {
                    podForNextPlayer = pod;
                    return;
                }
            }

            podForNextPlayer = await CreateNewEscapePodAsync();
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

    public async Task SetupIntroSequenceAsync(Player playerA, Player playerB)
    {
        EscapePodEntity escapePod = escapePodsByPlayerId[playerA.Id];

        if (escapePodsByPlayerId[playerB.Id] != escapePod)
        {
            // The two players are in different escape pods, so don't sync the escape pod fire.
            // In this case the fire can just exist locally for each client, with no sync
            return;
        }

        // This entity only needs to exist for the players in the escape pod for a moment, so we forget it,
        // both on the server-side and on the client-side when the intro sequence finishes.
        Entity fireEntity = new PathBasedChildEntity("Intro", new NitroxId(), NitroxTechType.None, null, escapePod.Id, []);

        SpawnEntities spawnEntities = new(fireEntity);
        await packetSender.SendPacketAsync(spawnEntities, playerA.SessionId);
        await packetSender.SendPacketAsync(spawnEntities, playerB.SessionId);
    }

    private static bool HasEmptySlot([NotNullWhen(true)] EscapePodEntity? pod)
    {
        if (pod == null)
        {
            return false;
        }
        return pod.Players.Count < PLAYERS_PER_ESCAPEPOD;
    }
}

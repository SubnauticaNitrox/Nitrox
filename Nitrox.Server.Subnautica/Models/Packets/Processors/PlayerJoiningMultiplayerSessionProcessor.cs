using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PlayerJoiningMultiplayerSessionProcessor(
    StoryTimingService storyTimingService,
    PlayerRepository playerRepository,
    EntitySimulation entitySimulation,
    WorldEntityManager worldEntityManager,
    EscapePodService escapePodService,
    EntityRegistry entityRegistry,
    SessionSettings sessionSettings,
    IOptions<SubnauticaServerOptions> optionsProvider,
    ILogger<PlayerJoiningMultiplayerSessionProcessor> logger)
    : IAnonPacketProcessor<PlayerJoiningMultiplayerSession>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly EscapePodService escapePodService = escapePodService;
    private readonly PlayerRepository playerRepository = playerRepository;
    private readonly IOptions<SubnauticaServerOptions> optionsProvider = optionsProvider;
    private readonly StoryTimingService storyTimingService = storyTimingService;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly ILogger<PlayerJoiningMultiplayerSessionProcessor> logger = logger;
    private readonly SessionSettings sessionSettings;

    public async Task Process(AnonProcessorContext context, PlayerJoiningMultiplayerSession packet)
    {
        // TODO: USE DATABASE - Assign escape pod
        // PeerId player = playerRepository.AddConnectedPlayer(context.Sender, packet.ReservationKey, out bool wasBrandNewPlayer);
        // NitroxId assignedEscapePodId = escapePodService.AssignPlayerToEscapePod(player.Id, out Optional<EscapePodWorldEntity> newlyCreatedEscapePod);
        // if (wasBrandNewPlayer)
        // {
        //     player.SubRootId = assignedEscapePodId;
        // }
        // if (newlyCreatedEscapePod.HasValue)
        // {
        //     SpawnEntities spawnNewEscapePod = new(newlyCreatedEscapePod.Value);
        //     playerService.SendPacketToOtherPlayers(spawnNewEscapePod, player);
        // }

        // TODO: FIX - need to provide endpoint to processor context
        // Make players on localhost admin by default.
        // if (connection.Endpoint.Address.IsLocalhost())
        // {
        //     Log.Info($"Granted admin to '{player.Name}' because they're playing on the host machine");
        //     player.Permissions = Perms.ADMIN;
        // }

        // TODO: USE DATABASE
        // List<SimulatedEntity> simulations = entitySimulation.AssignGlobalRootEntitiesAndGetData(player);
        //
        // player.Entity = wasBrandNewPlayer ? SetupPlayerEntity(player) : RespawnExistingEntity(player);
        //
        // List<GlobalRootEntity> globalRootEntities = worldEntityManager.GetGlobalRootEntities(true);
        // bool isFirstPlayer = playerService.GetConnectedPlayersAsync().Count == 1;
        //
        // InitialPlayerSync initialPlayerSync = new(player.GameObjectId,
        //                                           wasBrandNewPlayer,
        //                                           assignedEscapePodId,
        //                                           player.EquippedItems,
        //                                           player.UsedItems,
        //                                           player.QuickSlotsBindingIds,
        //                                           // TODO: USE DATABASE HERE
        //                                           default,
        //                                           default,
        //                                           // world.GameData.PDAState.GetInitialPDAData(),
        //                                           // world.GameData.StoryGoals.GetInitialStoryGoalData(scheduleKeeper, player),
        //                                           player.Position,
        //                                           player.Rotation,
        //                                           player.SubRootId,
        //                                           player.Stats,
        //                                           GetOtherPlayers(player),
        //                                           globalRootEntities,
        //                                           simulations,
        //                                           player.GameMode,
        //                                           player.Permissions,
        //                                           wasBrandNewPlayer ? IntroCinematicMode.LOADING : IntroCinematicMode.COMPLETED,
        //                                           new SubnauticaPlayerPreferences(new Dictionary<string, PingInstancePreference>(player.PingInstancePreferences), player.PinnedRecipePreferences.ToList()),
        //                                           storyTimingService.GetTimeData(),
        //                                           isFirstPlayer,
        //                                           BuildingManager.GetEntitiesOperations(globalRootEntities),
        //                                           optionsProvider.Value.KeepInventoryOnDeath,
        //                                           sessionSettings
        // );
        //
        // player.SendPacket(initialPlayerSync);
    }

    private PlayerWorldEntity SetupPlayerEntity(NitroxServer.Player player)
    {
        NitroxTransform transform = new(player.Position, player.Rotation, NitroxVector3.One);

        PlayerWorldEntity playerEntity = new(transform, 0, null, false, player.GameObjectId, NitroxTechType.None, null, null, new List<Entity>());
        entityRegistry.AddOrUpdate(playerEntity);
        worldEntityManager.TrackEntityInTheWorld(playerEntity);
        return playerEntity;
    }

    private PlayerWorldEntity RespawnExistingEntity(NitroxServer.Player player)
    {
        if (entityRegistry.TryGetEntityById(player.PlayerContext.PlayerNitroxId, out PlayerWorldEntity playerWorldEntity))
        {
            return playerWorldEntity;
        }
        logger.ZLogError($"Unable to find player entity for {player.Name}. Re-creating one");
        return SetupPlayerEntity(player);
    }
}

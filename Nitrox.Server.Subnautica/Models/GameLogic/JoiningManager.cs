using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.MultiplayerSession;
using Nitrox.Server.Subnautica.Models.AppEvents;
using Nitrox.Server.Subnautica.Models.AppEvents.Core;
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

// TODO: Refactor this to a QueuingBackgroundService to simplify state tracking.
internal sealed class JoiningManager(
    IPacketSender packetSender,
    PlayerService playerService,
    TaskTrackingService taskTrackingService,
    SessionManager sessionManager,
    WorldEntityManager worldEntityManager,
    PdaManager pdaManager,
    StoryManager storyManager,
    StoryScheduler storyScheduler,
    EntitySimulation entitySimulation,
    EscapePodManager escapePodManager,
    EntityRegistry entityRegistry,
    SessionSettings sessionSettings,
    IOptions<SubnauticaServerOptions> options,
    ILogger<JoiningManager> logger)
    : ISessionCleaner
{
    private readonly IPacketSender packetSender = packetSender;
    private readonly PlayerService playerService = playerService;
    private readonly TaskTrackingService taskTrackingService = taskTrackingService;
    private readonly SessionManager sessionManager = sessionManager;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly PdaManager pdaManager = pdaManager;
    private readonly StoryManager storyManager = storyManager;
    private readonly StoryScheduler storyScheduler = storyScheduler;
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly IOptions<SubnauticaServerOptions> options = options;
    private readonly ILogger<JoiningManager> logger = logger;
    private readonly EscapePodManager escapePodManager = escapePodManager;
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly SessionSettings sessionSettings = sessionSettings;

    private readonly ThreadSafeQueue<SessionId> joinQueue = new();
    private readonly Lock queueLocker = new(); // Necessary to avoid race conditions between JoinQueueLoop and AddToJoinQueue
    private bool queueActive;
    public Action? SyncFinishedCallback { get; private set; }

    private async Task JoinQueueLoop()
    {
        while (true)
        {
            lock (queueLocker)
            {
                if (joinQueue.Count == 0)
                {
                    queueActive = false;
                    return;
                }
            }

            try
            {
                SessionId sessionId = joinQueue.Dequeue();
                string? name = playerService.GetPlayerReservation(sessionId)?.PlayerName;
                if (name == null)
                {
                    continue;
                }

                // Do this after dequeuing because everyone's position shifts forward
                SessionId[] array = [.. joinQueue];
                for (int i = 0; i < array.Length; i++)
                {
                    SessionId s = array[i];
                    await packetSender.SendPacketAsync(new JoinQueueInfo(i + 1, options.Value.InitialSyncTimeout), s);
                }

                logger.ZLogInformation($"Starting sync for player '{name}' #{sessionId}");
                await SendInitialSyncAsync(sessionId);

                using CancellationTokenSource source = new(options.Value.InitialSyncTimeout);
                bool syncFinished = false;

                SyncFinishedCallback = () => { syncFinished = true; };

                while (!syncFinished && sessionManager.IsConnected(sessionId) && !source.IsCancellationRequested)
                {
                    await Task.Delay(10, source.Token);
                }

                if (!sessionManager.IsConnected(sessionId))
                {
                    logger.ZLogInformation($"Player {name} disconnected while syncing");
                }
                else if (source.IsCancellationRequested)
                {
                    logger.ZLogInformation($"Initial sync timed out for player {name}");
                    SyncFinishedCallback = null;

                    if (sessionManager.IsConnected(sessionId))
                    {
                        await packetSender.SendPacketAsync(new PlayerKicked("Initial sync took too long and timed out"), sessionId);
                    }
                }
                else
                {
                    logger.ZLogInformation($"Player '{name}' joined successfully. Remaining requests: {joinQueue.Count}");
                    BroadcastPlayerJoined(sessionId);
                }
            }
            catch (Exception e)
            {
                logger.ZLogInformation($"Unexpected error during player connection inside the join queue: {e}");
            }
        }
    }

    public void AddToJoinQueue(SessionId sessionId)
    {
        // Necessary to avoid race conditions between JoinQueueLoop and AddToJoinQueue
        lock (queueLocker)
        {
            logger.ZLogInformation($"Added player {playerService.GetPlayerReservation(sessionId)?.PlayerName} to queue");
            joinQueue.Enqueue(sessionId);

            if (queueActive)
            {
                packetSender.SendPacketAsync(new JoinQueueInfo(joinQueue.Count, options.Value.InitialSyncTimeout), sessionId);
            }
            else
            {
                // It may be possible to use the task's status itself for this,
                // but the ContinueWithHandleError callback might cause issues
                queueActive = true;
                Task.Run(JoinQueueLoop).ContinueWithHandleError();
            }
        }
    }

    private async Task SendInitialSyncAsync(SessionId sessionId)
    {
        bool isBrandNewPlayer = playerService.Get<IsNewPlayerProperty>(sessionId).Value;
        (NitroxId assignedEscapePodId, EscapePodEntity? newlyCreatedEscapePod) = await escapePodManager.AssignPlayerToEscapePodAsync(playerService.GetPeerId(sessionId));

        if (isBrandNewPlayer)
        {
            playerService.Get<SubRootIdProperty>(sessionId).Value = assignedEscapePodId;
        }
        if (newlyCreatedEscapePod is { } validEscapePod)
        {
            SpawnEntities spawnNewEscapePod = new(validEscapePod);
            await packetSender.SendPacketToOthersAsync(spawnNewEscapePod, sessionId);
        }

        // Make players on localhost admin by default.
        if (options.Value.LocalhostIsAdmin && sessionManager.GetEndPoint(sessionId)?.Address.IsLocalhost() == true)
        {
            logger.ZLogInformation($"Granted admin to '{playerService.Get<NameProperty>(sessionId).Value}' because they're playing on the host machine");
            playerService.Get<PermissionsProperty>(sessionId).Value = Perms.ADMIN;
        }

        List<SimulatedEntity> simulations = entitySimulation.AssignGlobalRootEntitiesAndGetData(sessionId);

        playerService.Get<EntityProperty>(sessionId).Value = isBrandNewPlayer ? SetupNewPlayerEntity(sessionId) : RespawnExistingEntity(sessionId);

        List<GlobalRootEntity> globalRootEntities = worldEntityManager.GetGlobalRootEntities(true);
        bool isFirstPlayer = sessionManager.GetSessionCount() == 1;

        InitialPlayerSync initialPlayerSync = new(
            playerService.Get<GameObjectIdProperty>(sessionId).Value,
            isBrandNewPlayer,
            assignedEscapePodId,
            playerService.Get<EquippedItemsProperty>(sessionId).Value,
            playerService.Get<UsedItemsProperty>(sessionId).Value,
            playerService.Get<QuickSlotsProperty>(sessionId).Value.ToList(),
            pdaManager.GetInitialPDAData(),
            storyManager.GetInitialStoryGoalData(storyScheduler, sessionId),
            playerService.Get<PositionProperty>(sessionId).Value,
            playerService.Get<RotationProperty>(sessionId).Value,
            playerService.Get<SubRootIdProperty>(sessionId).Value,
            playerService.Get<StatsProperty>(sessionId).Value,
            GetOtherPlayers(sessionId),
            globalRootEntities,
            simulations,
            playerService.Get<GameModeProperty>(sessionId).Value,
            playerService.Get<PermissionsProperty>(sessionId).Value,
            isBrandNewPlayer ? IntroCinematicMode.LOADING : IntroCinematicMode.COMPLETED,
            new(new(playerService.Get<PingsProperty>(sessionId).Value), playerService.Get<PinnedRecipesProperty>(sessionId).Value.ToList()),
            storyManager.GetTimeData(),
            isFirstPlayer,
            BuildingManager.GetEntitiesOperations(globalRootEntities),
            options.Value.KeepInventoryOnDeath,
            sessionSettings,
            playerService.Get<InPrecursorProperty>(sessionId).Value,
            playerService.Get<DisplaySurfaceWaterProperty>(sessionId).Value,
            options.Value.MarkDeathPointsWithBeacon
        );

        await packetSender.SendPacketAsync(initialPlayerSync, sessionId);

        IEnumerable<PlayerContext> GetOtherPlayers(SessionId player) => playerService.GetSessionsExcept(player).Select(p => playerService.Get<ContextProperty>(p).Value);

        PlayerEntity SetupNewPlayerEntity(SessionId player)
        {
            NitroxTransform transform = new(playerService.Get<PositionProperty>(player).Value, playerService.Get<RotationProperty>(player).Value, NitroxVector3.One);

            PlayerEntity playerEntity = new(transform, 0, null, false, playerService.Get<GameObjectIdProperty>(player).Value, NitroxTechType.None, null, playerService.Get<SubRootIdProperty>(player).Value, []);
            entityRegistry.AddOrUpdate(playerEntity);
            worldEntityManager.TrackEntityInTheWorld(playerEntity);
            return playerEntity;
        }

        PlayerEntity RespawnExistingEntity(SessionId player)
        {
            if (entityRegistry.TryGetEntityById(playerService.Get<GameObjectIdProperty>(player).Value, out PlayerEntity playerWorldEntity))
            {
                return playerWorldEntity;
            }
            logger.ZLogError($"Unable to find player entity for {playerService.Get<NameProperty>(player).Value}. Re-creating one");
            return SetupNewPlayerEntity(player);
        }
    }

    private void BroadcastPlayerJoined(SessionId sessionId)
    {
        PlayerJoinedMultiplayerSession playerJoinedPacket = new(playerService.Get<ContextProperty>(sessionId).Value, playerService.Get<SubRootIdProperty>(sessionId).Value, playerService.Get<EntityProperty>(sessionId).Value);
        taskTrackingService.TryTrack(packetSender.SendPacketToOthersAsync(playerJoinedPacket, sessionId).AsTask());
    }

    Task IEvent<ISessionCleaner.Args>.OnEventAsync(ISessionCleaner.Args args)
    {
        // They may have been queued, so just erase their entry
        joinQueue.RemoveWhere(sessionId => Equals(sessionId, args.Session.Id));
        return Task.CompletedTask;
    }
}

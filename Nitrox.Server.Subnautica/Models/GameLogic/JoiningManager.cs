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
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

internal sealed class JoiningManager(
    IPacketSender packetSender,
    PlayerManager playerManager,
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
    private readonly PlayerManager playerManager = playerManager;
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

    private readonly ThreadSafeQueue<(SessionId, string)> joinQueue = new();
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
                (SessionId sessionId, string reservationKey) = joinQueue.Dequeue();
                string? name = playerManager.GetPlayerContext(reservationKey)?.PlayerName;
                if (name == null)
                {
                    continue;
                }

                // Do this after dequeuing because everyone's position shifts forward
                (SessionId, string)[] array = [.. joinQueue];
                for (int i = 0; i < array.Length; i++)
                {
                    (SessionId s, _) = array[i];
                    await packetSender.SendPacketAsync(new JoinQueueInfo(i + 1, options.Value.InitialSyncTimeout), s);
                }

                logger.ZLogInformation($"Starting sync for player {name}");
                SendInitialSync(sessionId, reservationKey);

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
                    playerManager.RemovePlayer(sessionId);
                }
                else
                {
                    logger.ZLogInformation($"Player {name} joined successfully. Remaining requests: {joinQueue.Count}");
                    if (!playerManager.TryGetPlayerBySessionId(sessionId, out Player? player))
                    {
                        throw new Exception($"Failed to get player object for session #{sessionId}");
                    }
                    BroadcastPlayerJoined(player);
                }
            }
            catch (Exception e)
            {
                logger.ZLogInformation($"Unexpected error during player connection inside the join queue: {e}");
            }
        }
    }

    public void AddToJoinQueue(SessionId sessionId, string reservationKey)
    {
        // Necessary to avoid race conditions between JoinQueueLoop and AddToJoinQueue
        lock (queueLocker)
        {
            logger.ZLogInformation($"Added player {playerManager.GetPlayerContext(reservationKey)?.PlayerName} to queue");
            joinQueue.Enqueue((sessionId, reservationKey));

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

    private void SendInitialSync(SessionId sessionId, string reservationKey)
    {
        Player player = playerManager.CreatePlayerData(sessionId, reservationKey, out bool wasBrandNewPlayer);
        NitroxId assignedEscapePodId = escapePodManager.AssignPlayerToEscapePod(player.Id, out Optional<EscapePodEntity> newlyCreatedEscapePod);

        if (wasBrandNewPlayer)
        {
            player.SubRootId = assignedEscapePodId;
        }

        if (newlyCreatedEscapePod.HasValue)
        {
            SpawnEntities spawnNewEscapePod = new(newlyCreatedEscapePod.Value);
            packetSender.SendPacketToOthersAsync(spawnNewEscapePod, sessionId);
        }

        // TODO: Remove this code when security of player login is improved by https://github.com/SubnauticaNitrox/Nitrox/issues/1996
        // We need to reset permissions on join, otherwise players can impersonate an admin easily.
        player.Permissions = options.Value.DefaultPlayerPerm;

        // Make players on localhost admin by default.
        if (options.Value.LocalhostIsAdmin && sessionManager.GetEndPoint(sessionId)?.Address.IsLocalhost() == true)
        {
            logger.ZLogInformation($"Granted admin to '{player.Name}' because they're playing on the host machine");
            player.Permissions = Perms.ADMIN;
        }

        List<SimulatedEntity> simulations = entitySimulation.AssignGlobalRootEntitiesAndGetData(player);

        player.Entity = wasBrandNewPlayer ? SetupNewPlayerEntity(player) : RespawnExistingEntity(player);

        List<GlobalRootEntity> globalRootEntities = worldEntityManager.GetGlobalRootEntities(true);
        bool isFirstPlayer = playerManager.GetConnectedPlayers().Count == 1;

        InitialPlayerSync initialPlayerSync = new(player.GameObjectId,
            wasBrandNewPlayer,
            assignedEscapePodId,
            player.EquippedItems,
            player.UsedItems,
            player.QuickSlotsBindingIds,
            pdaManager.GetInitialPDAData(),
            storyManager.GetInitialStoryGoalData(storyScheduler, player),
            player.Position,
            player.Rotation,
            player.SubRootId,
            player.Stats,
            GetOtherPlayers(player),
            globalRootEntities,
            simulations,
            player.GameMode,
            player.Permissions,
            wasBrandNewPlayer ? IntroCinematicMode.LOADING : IntroCinematicMode.COMPLETED,
            new(new(player.PingInstancePreferences), player.PinnedRecipePreferences.ToList()),
            storyManager.GetTimeData(),
            isFirstPlayer,
            BuildingManager.GetEntitiesOperations(globalRootEntities),
            options.Value.KeepInventoryOnDeath,
            sessionSettings,
            player.InPrecursor,
            player.DisplaySurfaceWater
        );

        packetSender.SendPacketAsync(initialPlayerSync, player.SessionId);

        IEnumerable<PlayerContext> GetOtherPlayers(Player player)
        {
            return playerManager.GetConnectedPlayers().Where(p => p != player).Select(p => p.PlayerContext);
        }

        PlayerEntity SetupNewPlayerEntity(Player player)
        {
            NitroxTransform transform = new(player.Position, player.Rotation, NitroxVector3.One);

            PlayerEntity playerEntity = new(transform, 0, null, false, player.GameObjectId, NitroxTechType.None, null, null, new List<Entity>());
            entityRegistry.AddOrUpdate(playerEntity);
            worldEntityManager.TrackEntityInTheWorld(playerEntity);
            return playerEntity;
        }

        PlayerEntity RespawnExistingEntity(Player player)
        {
            if (entityRegistry.TryGetEntityById(player.PlayerContext.PlayerNitroxId, out PlayerEntity playerWorldEntity))
            {
                return playerWorldEntity;
            }
            logger.ZLogError($"Unable to find player entity for {player.Name}. Re-creating one");
            return SetupNewPlayerEntity(player);
        }
    }

    private void BroadcastPlayerJoined(Player player)
    {
        PlayerJoinedMultiplayerSession playerJoinedPacket = new(player.PlayerContext, player.SubRootId, player.Entity);
        packetSender.SendPacketToOthersAsync(playerJoinedPacket, player.SessionId);
    }

    public Task OnEventAsync(ISessionCleaner.Args args)
    {
        // They may have been queued, so just erase their entry
        joinQueue.RemoveWhere(tuple => Equals(tuple.Item1, args.Session.Id));
        return Task.CompletedTask;
    }
}

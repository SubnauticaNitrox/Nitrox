using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.MultiplayerSession;
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

internal sealed class JoiningManager
{
    private readonly PlayerManager playerManager;
    private readonly WorldEntityManager worldEntityManager;
    private readonly PdaManager pdaManager;
    private readonly StoryManager storyManager;
    private readonly StoryScheduler storyScheduler;
    private readonly EntitySimulation entitySimulation;
    private readonly IOptions<SubnauticaServerOptions> options;
    private readonly ILogger<JoiningManager> logger;
    private readonly EscapePodManager escapePodManager;
    private readonly EntityRegistry entityRegistry;
    private readonly SessionSettings sessionSettings;

    private readonly ThreadSafeQueue<(INitroxConnection, string)> joinQueue = new();
    private readonly Lock queueLocker = new(); // Necessary to avoid race conditions between JoinQueueLoop and AddToJoinQueue
    private bool queueActive;
    public Action? SyncFinishedCallback { get; private set; }

    public JoiningManager(PlayerManager playerManager, WorldEntityManager worldEntityManager, PdaManager pdaManager, StoryManager storyManager, StoryScheduler storyScheduler, EntitySimulation entitySimulation, EscapePodManager escapePodManager, EntityRegistry entityRegistry, SessionSettings sessionSettings, IOptions<SubnauticaServerOptions> options, ILogger<JoiningManager> logger)
    {
        this.playerManager = playerManager;
        this.worldEntityManager = worldEntityManager;
        this.pdaManager = pdaManager;
        this.storyManager = storyManager;
        this.storyScheduler = storyScheduler;
        this.entitySimulation = entitySimulation;
        this.options = options;
        this.logger = logger;
        this.escapePodManager = escapePodManager;
        this.entityRegistry = entityRegistry;
        this.sessionSettings = sessionSettings;
    }

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
                (INitroxConnection connection, string reservationKey) = joinQueue.Dequeue();
                string name = playerManager.GetPlayerContext(reservationKey).PlayerName;

                // Do this after dequeuing because everyone's position shifts forward
                (INitroxConnection, string)[] array = [.. joinQueue];
                for (int i = 0; i < array.Length; i++)
                {
                    (INitroxConnection c, _) = array[i];
                    c.SendPacket(new JoinQueueInfo(i + 1, options.Value.InitialSyncTimeout));
                }

                logger.ZLogInformation($"Starting sync for player {name}");
                SendInitialSync(connection, reservationKey);

                using CancellationTokenSource source = new(options.Value.InitialSyncTimeout);
                bool syncFinished = false;

                SyncFinishedCallback = () => { syncFinished = true; };

                while (!syncFinished &&
                       connection.State != NitroxConnectionState.Disconnected &&
                       !source.IsCancellationRequested)
                {
                    await Task.Delay(10);
                }

                if (connection.State == NitroxConnectionState.Disconnected)
                {
                    logger.ZLogInformation($"Player {name} disconnected while syncing");
                }
                else if (source.IsCancellationRequested)
                {
                    logger.ZLogInformation($"Initial sync timed out for player {name}");
                    SyncFinishedCallback = null;

                    if (connection.State == NitroxConnectionState.Connected)
                    {
                        connection.SendPacket(new PlayerKicked("Initial sync took too long and timed out"));
                    }
                    playerManager.PlayerDisconnected(connection);
                }
                else
                {
                    logger.ZLogInformation($"Player {name} joined successfully. Remaining requests: {joinQueue.Count}");
                    BroadcastPlayerJoined(playerManager.GetPlayer(connection));
                }
            }
            catch (Exception e)
            {
                logger.ZLogInformation($"Unexpected error during player connection inside the join queue: {e}");
            }
        }
    }

    public void AddToJoinQueue(INitroxConnection connection, string reservationKey)
    {
        // Necessary to avoid race conditions between JoinQueueLoop and AddToJoinQueue
        lock (queueLocker)
        {
            logger.ZLogInformation($"Added player {playerManager.GetPlayerContext(reservationKey)?.PlayerName} to queue");
            joinQueue.Enqueue((connection, reservationKey));

            if (queueActive)
            {
                connection.SendPacket(new JoinQueueInfo(joinQueue.Count, options.Value.InitialSyncTimeout));
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

    private void SendInitialSync(INitroxConnection connection, string reservationKey)
    {
        Player player = playerManager.PlayerConnected(connection, reservationKey, out bool wasBrandNewPlayer);
        NitroxId assignedEscapePodId = escapePodManager.AssignPlayerToEscapePod(player.Id, out Optional<EscapePodEntity> newlyCreatedEscapePod);

        if (wasBrandNewPlayer)
        {
            player.SubRootId = assignedEscapePodId;
        }

        if (newlyCreatedEscapePod.HasValue)
        {
            SpawnEntities spawnNewEscapePod = new(newlyCreatedEscapePod.Value);
            playerManager.SendPacketToOtherPlayers(spawnNewEscapePod, player);
        }

        // TODO: Remove this code when security of player login is improved by https://github.com/SubnauticaNitrox/Nitrox/issues/1996
        // We need to reset permissions on join, otherwise players can impersonate an admin easily.
        player.Permissions = options.Value.DefaultPlayerPerm;

        // Make players on localhost admin by default.
        if (options.Value.LocalhostIsAdmin && connection.Endpoint.Address.IsLocalhost())
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

        player.SendPacket(initialPlayerSync);

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

    public void JoiningPlayerDisconnected(INitroxConnection connection)
    {
        // They may have been queued, so just erase their entry
        joinQueue.RemoveWhere(tuple => Equals(tuple.Item1, connection));
    }

    public void BroadcastPlayerJoined(Player player)
    {
        PlayerJoinedMultiplayerSession playerJoinedPacket = new(player.PlayerContext, player.SubRootId, player.Entity);
        playerManager.SendPacketToOtherPlayers(playerJoinedPacket, player);
    }
}

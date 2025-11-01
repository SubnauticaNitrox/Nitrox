using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Serialization;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.MultiplayerSession;
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.Serialization.World;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

public sealed class JoiningManager
{
    private readonly PlayerManager playerManager;
    private readonly SubnauticaServerConfig serverConfig;
    private readonly World world;
    private readonly SessionSettings sessionSettings;

    private readonly ThreadSafeQueue<(INitroxConnection, string)> joinQueue = new();
    private readonly Lock queueLocker = new(); // Necessary to avoid race conditions between JoinQueueLoop and AddToJoinQueue
    private bool queueActive;
    public Action SyncFinishedCallback { get; private set; }

    public JoiningManager(PlayerManager playerManager, SubnauticaServerConfig serverConfig, World world, SessionSettings sessionSettings)
    {
        this.playerManager = playerManager;
        this.serverConfig = serverConfig;
        this.world = world;
        this.sessionSettings = sessionSettings;
    }

    private async Task JoinQueueLoop()
    {
        // It may be possible to use the task's status itself for this,
        // but the ContinueWithHandleError callback might cause issues
        queueActive = true;

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
                    c.SendPacket(new JoinQueueInfo(i + 1, serverConfig.InitialSyncTimeout));
                }

                Log.Info($"Starting sync for player {name}");
                SendInitialSync(connection, reservationKey);

                using CancellationTokenSource source = new(serverConfig.InitialSyncTimeout);
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
                    Log.Info($"Player {name} disconnected while syncing");
                }
                else if (source.IsCancellationRequested)
                {
                    Log.Info($"Initial sync timed out for player {name}");
                    SyncFinishedCallback = null;

                    if (connection.State == NitroxConnectionState.Connected)
                    {
                        connection.SendPacket(new PlayerKicked("Initial sync took too long and timed out"));
                    }
                    playerManager.PlayerDisconnected(connection);
                }
                else
                {
                    Log.Info($"Player {name} joined successfully. Remaining requests: {joinQueue.Count}");
                    BroadcastPlayerJoined(playerManager.GetPlayer(connection));
                }
            }
            catch (Exception e)
            {
                Log.Error($"Unexpected error during player connection inside the join queue: {e}");
            }
        }
    }

    public void AddToJoinQueue(INitroxConnection connection, string reservationKey)
    {
        // Necessary to avoid race conditions between JoinQueueLoop and AddToJoinQueue
        lock (queueLocker)
        {
            Log.Info($"Added player {playerManager.GetPlayerContext(reservationKey).PlayerName} to queue");
            joinQueue.Enqueue((connection, reservationKey));

            if (queueActive)
            {
                connection.SendPacket(new JoinQueueInfo(joinQueue.Count, serverConfig.InitialSyncTimeout));
            }
            else
            {
                Task.Run(JoinQueueLoop).ContinueWithHandleError();
            }
        }
    }

    private void SendInitialSync(INitroxConnection connection, string reservationKey)
    {
        Player player = playerManager.PlayerConnected(connection, reservationKey, out bool wasBrandNewPlayer);
        NitroxId assignedEscapePodId = world.EscapePodManager.AssignPlayerToEscapePod(player.Id, out Optional<EscapePodEntity> newlyCreatedEscapePod);

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
        player.Permissions = serverConfig.DefaultPlayerPerm;

        // Make players on localhost admin by default.
        if (serverConfig.LocalhostIsAdmin && connection.Endpoint.Address.IsLocalhost())
        {
            Log.Info($"Granted admin to '{player.Name}' because they're playing on the host machine");
            player.Permissions = Perms.ADMIN;
        }

        List<SimulatedEntity> simulations = world.EntitySimulation.AssignGlobalRootEntitiesAndGetData(player);

        player.Entity = wasBrandNewPlayer ? SetupNewPlayerEntity(player) : RespawnExistingEntity(player);

        List<GlobalRootEntity> globalRootEntities = world.WorldEntityManager.GetGlobalRootEntities(true);
        bool isFirstPlayer = playerManager.GetConnectedPlayers().Count == 1;

        InitialPlayerSync initialPlayerSync = new(player.GameObjectId,
            wasBrandNewPlayer,
            assignedEscapePodId,
            player.EquippedItems,
            player.UsedItems,
            player.QuickSlotsBindingIds,
            world.GameData.PDAState.GetInitialPDAData(),
            world.GameData.StoryGoals.GetInitialStoryGoalData(world.ScheduleKeeper, player),
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
            world.StoryManager.GetTimeData(),
            isFirstPlayer,
            BuildingManager.GetEntitiesOperations(globalRootEntities),
            serverConfig.KeepInventoryOnDeath,
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
            world.EntityRegistry.AddOrUpdate(playerEntity);
            world.WorldEntityManager.TrackEntityInTheWorld(playerEntity);
            return playerEntity;
        }

        PlayerEntity RespawnExistingEntity(Player player)
        {
            if (world.EntityRegistry.TryGetEntityById(player.PlayerContext.PlayerNitroxId, out PlayerEntity playerWorldEntity))
            {
                return playerWorldEntity;
            }
            Log.Error($"Unable to find player entity for {player.Name}. Re-creating one");
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

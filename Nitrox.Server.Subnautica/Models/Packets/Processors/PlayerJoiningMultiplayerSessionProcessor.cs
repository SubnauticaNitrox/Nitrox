using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Networking;
using Nitrox.Model.Serialization;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.MultiplayerSession;
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Serialization.World;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    public class PlayerJoiningMultiplayerSessionProcessor : UnauthenticatedPacketProcessor<PlayerJoiningMultiplayerSession>
    {
        private readonly PlayerManager playerManager;
        private readonly ScheduleKeeper scheduleKeeper;
        private readonly StoryManager storyManager;
        private readonly World world;
        private readonly EntityRegistry entityRegistry;
        private readonly SubnauticaServerConfig serverConfig;
        private readonly NtpSyncer ntpSyncer;
        private readonly SessionSettings sessionSettings;

        public PlayerJoiningMultiplayerSessionProcessor(ScheduleKeeper scheduleKeeper, StoryManager storyManager, PlayerManager playerManager, World world, EntityRegistry entityRegistry, SubnauticaServerConfig serverConfig, NtpSyncer ntpSyncer, SessionSettings sessionSettings)
        {
            this.scheduleKeeper = scheduleKeeper;
            this.storyManager = storyManager;
            this.playerManager = playerManager;
            this.world = world;
            this.entityRegistry = entityRegistry;
            this.serverConfig = serverConfig;
            this.ntpSyncer = ntpSyncer;
            this.sessionSettings = sessionSettings;
        }

        public override void Process(PlayerJoiningMultiplayerSession packet, INitroxConnection connection)
        {
            Player player = playerManager.PlayerConnected(connection, packet.ReservationKey, out bool wasBrandNewPlayer);
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

            player.Entity = wasBrandNewPlayer ? SetupPlayerEntity(player) : RespawnExistingEntity(player);

            List<GlobalRootEntity> globalRootEntities = world.WorldEntityManager.GetGlobalRootEntities(true);
            bool isFirstPlayer = playerManager.GetConnectedPlayers().Count == 1;

            InitialPlayerSync initialPlayerSync = new(player.GameObjectId,
                wasBrandNewPlayer,
                assignedEscapePodId,
                player.EquippedItems,
                player.UsedItems,
                player.QuickSlotsBindingIds,
                world.GameData.PDAState.GetInitialPDAData(),
                world.GameData.StoryGoals.GetInitialStoryGoalData(scheduleKeeper, player),
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
                serverConfig.KeepInventoryOnDeath,
                sessionSettings,
                player.InPrecursor,
                player.DisplaySurfaceWater
            );

            player.SendPacket(initialPlayerSync);
        }

        private IEnumerable<PlayerContext> GetOtherPlayers(Player player)
        {
            return playerManager.GetConnectedPlayers().Where(p => p != player)
                                                      .Select(p => p.PlayerContext);
        }

        private PlayerEntity SetupPlayerEntity(Player player)
        {
            NitroxTransform transform = new(player.Position, player.Rotation, NitroxVector3.One);

            PlayerEntity playerEntity = new PlayerEntity(transform, 0, null, false, player.GameObjectId, NitroxTechType.None, null, null, new List<Entity>());
            entityRegistry.AddOrUpdate(playerEntity);
            world.WorldEntityManager.TrackEntityInTheWorld(playerEntity);
            return playerEntity;
        }

        private PlayerEntity RespawnExistingEntity(Player player)
        {
            if (entityRegistry.TryGetEntityById(player.PlayerContext.PlayerNitroxId, out PlayerEntity playerWorldEntity))
            {
                return playerWorldEntity;
            }
            Log.Error($"Unable to find player entity for {player.Name}. Re-creating one");
            return SetupPlayerEntity(player);
        }
    }
}

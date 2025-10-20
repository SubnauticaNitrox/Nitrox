using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.MultiplayerSession;
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    internal sealed class PlayerJoiningMultiplayerSessionProcessor : UnauthenticatedPacketProcessor<PlayerJoiningMultiplayerSession>
    {
        private readonly PlayerManager playerManager;
        private readonly StoryScheduler storyScheduler;
        private readonly StoryManager storyManager;
        private readonly EntityRegistry entityRegistry;
        private readonly WorldEntityManager worldEntityManager;
        private readonly EntitySimulation entitySimulation;
        private readonly EscapePodManager escapePodManager;
        private readonly IOptions<SubnauticaServerOptions> options;
        private readonly PdaManager pdaManager;
        private readonly SessionSettings sessionSettings;
        private readonly ILogger<PlayerJoiningMultiplayerSessionProcessor> logger;

        public PlayerJoiningMultiplayerSessionProcessor(StoryScheduler storyScheduler, StoryManager storyManager, PlayerManager playerManager, EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, EntitySimulation entitySimulation, EscapePodManager escapePodManager, IOptions<SubnauticaServerOptions> options, PdaManager pdaManager, SessionSettings sessionSettings, ILogger<PlayerJoiningMultiplayerSessionProcessor> logger)
        {
            this.storyScheduler = storyScheduler;
            this.storyManager = storyManager;
            this.playerManager = playerManager;
            this.entityRegistry = entityRegistry;
            this.worldEntityManager = worldEntityManager;
            this.entitySimulation = entitySimulation;
            this.escapePodManager = escapePodManager;
            this.options = options;
            this.pdaManager = pdaManager;
            this.sessionSettings = sessionSettings;
            this.logger = logger;
        }

        public override void Process(PlayerJoiningMultiplayerSession packet, INitroxConnection connection)
        {
            Player player = playerManager.PlayerConnected(connection, packet.ReservationKey, out bool wasBrandNewPlayer);
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

            player.Entity = wasBrandNewPlayer ? SetupPlayerEntity(player) : RespawnExistingEntity(player);

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
        }

        private IEnumerable<PlayerContext> GetOtherPlayers(Player player)
        {
            return playerManager.GetConnectedPlayers().Where(p => p != player)
                                                      .Select(p => p.PlayerContext);
        }

        private PlayerEntity SetupPlayerEntity(Player player)
        {
            NitroxTransform transform = new(player.Position, player.Rotation, NitroxVector3.One);

            PlayerEntity playerEntity = new(transform, 0, null, false, player.GameObjectId, NitroxTechType.None, null, null, new List<Entity>());
            entityRegistry.AddOrUpdate(playerEntity);
            worldEntityManager.TrackEntityInTheWorld(playerEntity);
            return playerEntity;
        }

        private PlayerEntity RespawnExistingEntity(Player player)
        {
            if (entityRegistry.TryGetEntityById(player.PlayerContext.PlayerNitroxId, out PlayerEntity playerWorldEntity))
            {
                return playerWorldEntity;
            }
            logger.ZLogError($"Unable to find player entity for {player.Name}. Re-creating one");
            return SetupPlayerEntity(player);
        }
    }
}

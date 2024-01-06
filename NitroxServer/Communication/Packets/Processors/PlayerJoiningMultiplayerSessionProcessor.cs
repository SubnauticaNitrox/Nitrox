using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;
using NitroxServer.Serialization.World;

namespace NitroxServer.Communication.Packets.Processors
{
    public class PlayerJoiningMultiplayerSessionProcessor : UnauthenticatedPacketProcessor<PlayerJoiningMultiplayerSession>
    {
        private readonly PlayerManager playerManager;
        private readonly ScheduleKeeper scheduleKeeper;
        private readonly StoryManager storyManager;
        private readonly World world;
        private readonly EntityRegistry entityRegistry;
        private readonly BuildingManager buildingManager;

        public PlayerJoiningMultiplayerSessionProcessor(ScheduleKeeper scheduleKeeper, StoryManager storyManager, PlayerManager playerManager, World world, EntityRegistry entityRegistry, BuildingManager buildingManager)
        {
            this.scheduleKeeper = scheduleKeeper;
            this.storyManager = storyManager;
            this.playerManager = playerManager;
            this.world = world;
            this.entityRegistry = entityRegistry;
            this.buildingManager = buildingManager;
        }

        public override void Process(PlayerJoiningMultiplayerSession packet, NitroxConnection connection)
        {
            Player player = playerManager.PlayerConnected(connection, packet.ReservationKey, out bool wasBrandNewPlayer);
            NitroxId assignedEscapePodId = world.EscapePodManager.AssignPlayerToEscapePod(player.Id, out Optional<EscapePodWorldEntity> newlyCreatedEscapePod);

            if (newlyCreatedEscapePod.HasValue)
            {
                SpawnEntities spawnNewEscapePod = new(newlyCreatedEscapePod.Value);
                playerManager.SendPacketToOtherPlayers(spawnNewEscapePod, player);
            }

            List<EquippedItemData> equippedItems = player.GetEquipment();

            // Make players on localhost admin by default.
            if (connection.Endpoint.Address.IsLocalhost())
            {
                Log.Info($"Granted admin to '{player.Name}' because they're playing on the host machine");
                player.Permissions = Perms.ADMIN;
            }

            List<SimulatedEntity> simulations = world.EntitySimulation.AssignGlobalRootEntitiesAndGetData(player);

            player.Entity = wasBrandNewPlayer ? SetupPlayerEntity(player) : RespawnExistingEntity(player); ;

            List<GlobalRootEntity> globalRootEntities = world.WorldEntityManager.GetGlobalRootEntities(true);
            bool isFirstPlayer = playerManager.GetConnectedPlayers().Count == 1;

            InitialPlayerSync initialPlayerSync = new(player.GameObjectId,
                wasBrandNewPlayer,
                assignedEscapePodId,
                equippedItems,
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
                new(new(player.PingInstancePreferences), player.PinnedRecipePreferences.ToList()),
                storyManager.GetTimeData(),
                isFirstPlayer,
                BuildingManager.GetEntitiesOperations(globalRootEntities)
            );

            player.SendPacket(initialPlayerSync);
        }

        private IEnumerable<PlayerContext> GetOtherPlayers(Player player)
        {
            return playerManager.GetConnectedPlayers().Where(p => p != player)
                                                      .Select(p => p.PlayerContext);
        }

        private PlayerWorldEntity SetupPlayerEntity(Player player)
        {
            NitroxTransform transform = new(player.Position, player.Rotation, NitroxVector3.One);

            PlayerWorldEntity playerEntity = new PlayerWorldEntity(transform, 0, null, false, player.GameObjectId, NitroxTechType.None, null, null, new List<Entity>());
            entityRegistry.AddOrUpdate(playerEntity);
            world.WorldEntityManager.TrackEntityInTheWorld(playerEntity);
            return playerEntity;
        }

        private PlayerWorldEntity RespawnExistingEntity(Player player)
        {
            if (entityRegistry.TryGetEntityById(player.PlayerContext.PlayerNitroxId, out PlayerWorldEntity playerWorldEntity))
            {
                return playerWorldEntity;
            }
            Log.Error($"Unable to find player entity for {player.Name}. Re-creating one");
            return SetupPlayerEntity(player);
        }
    }
}

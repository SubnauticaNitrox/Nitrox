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
            List<NitroxTechType> techTypes = equippedItems.Select(equippedItem => equippedItem.TechType).ToList();

            PlayerJoinedMultiplayerSession playerJoinedPacket = new(player.PlayerContext, player.SubRootId, techTypes);
            playerManager.SendPacketToOtherPlayers(playerJoinedPacket, player);

            // Make players on localhost admin by default.
            if (connection.Endpoint.Address.IsLocalhost())
            {
                Log.Info($"Granted admin to '{player.Name}' because they're playing on the host machine");
                player.Permissions = Perms.ADMIN;
            }

            List<NitroxId> simulations = world.EntitySimulation.AssignGlobalRootEntities(player).ToList();

            if (wasBrandNewPlayer)
            {
                SetupPlayerEntity(player);
            }
            else
            {
                RespawnExistingEntity(player);
            }
            List<GlobalRootEntity> globalRootEntities = world.WorldEntityManager.GetGlobalRootEntities(true);

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
                BuildingManager.GetEntitiesOperations(globalRootEntities)
            );

            player.SendPacket(initialPlayerSync);
        }

        private IEnumerable<PlayerContext> GetOtherPlayers(Player player)
        {
            return playerManager.GetConnectedPlayers().Where(p => p != player)
                                                      .Select(p => p.PlayerContext);
        }

        private void SetupPlayerEntity(Player player)
        {
            NitroxTransform transform = new(player.Position, player.Rotation, NitroxVector3.One);

            PlayerWorldEntity playerEntity = new PlayerWorldEntity(transform, 0, null, false, player.GameObjectId, NitroxTechType.None, null, null, new List<Entity>());
            entityRegistry.AddEntity(playerEntity);
            world.WorldEntityManager.TrackEntityInTheWorld(playerEntity);
            playerManager.SendPacketToOtherPlayers(new SpawnEntities(playerEntity), player);
        }

        private void RespawnExistingEntity(Player player)
        {
            Optional<Entity> playerEntity = entityRegistry.GetEntityById(player.PlayerContext.PlayerNitroxId);

            if (playerEntity.HasValue)
            {
                playerManager.SendPacketToOtherPlayers(new SpawnEntities(playerEntity.Value, true), player);
            }
            else
            {
                Log.Error($"Unable to find player entity for {player.Name}. Re-creating one");
                SetupPlayerEntity(player);
            }
        }
    }
}

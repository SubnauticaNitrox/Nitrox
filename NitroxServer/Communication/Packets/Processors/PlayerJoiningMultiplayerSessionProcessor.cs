using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using NitroxServer.Serialization.World;

namespace NitroxServer.Communication.Packets.Processors
{
    public class PlayerJoiningMultiplayerSessionProcessor : UnauthenticatedPacketProcessor<PlayerJoiningMultiplayerSession>
    {
        private readonly PlayerManager playerManager;
        private readonly ScheduleKeeper scheduleKeeper;
        private readonly EventTriggerer eventTriggerer;
        private readonly World world;
        private readonly EntityRegistry entityRegistry;

        public PlayerJoiningMultiplayerSessionProcessor(ScheduleKeeper scheduleKeeper, EventTriggerer eventTriggerer, PlayerManager playerManager, World world, EntityRegistry entityRegistry)
        {
            this.scheduleKeeper = scheduleKeeper;
            this.eventTriggerer = eventTriggerer;
            this.playerManager = playerManager;
            this.world = world;
            this.entityRegistry = entityRegistry;
        }

        public override void Process(PlayerJoiningMultiplayerSession packet, NitroxConnection connection)
        {
            Player player = playerManager.PlayerConnected(connection, packet.ReservationKey, out bool wasBrandNewPlayer);
            NitroxId assignedEscapePodId = world.EscapePodManager.AssignPlayerToEscapePod(player.Id, out Optional<EscapePodWorldEntity> newlyCreatedEscapePod);

            if (newlyCreatedEscapePod.HasValue)
            {
                CellEntities spawnNewEscapePod = new(newlyCreatedEscapePod.Value);
                playerManager.SendPacketToOtherPlayers(spawnNewEscapePod, player);
            }

            List<EquippedItemData> equippedItems = player.GetEquipment();
            List<NitroxTechType> techTypes = equippedItems.Select(equippedItem => equippedItem.TechType).ToList();

            PlayerJoinedMultiplayerSession playerJoinedPacket = new(player.PlayerContext, player.SubRootId, techTypes);
            playerManager.SendPacketToOtherPlayers(playerJoinedPacket, player);

            // Make players on localhost admin by default.
            if (IPAddress.IsLoopback(connection.Endpoint.Address))
            {
                player.Permissions = Perms.ADMIN;
            }

            List<NitroxId> simulations = world.EntitySimulation.AssignGlobalRootEntities(player).ToList();
            IEnumerable<VehicleModel> vehicles = world.VehicleManager.GetVehicles();
            foreach (VehicleModel vehicle in vehicles)
            {
                if (world.SimulationOwnershipData.TryToAcquire(vehicle.Id, player, SimulationLockType.TRANSIENT))
                {
                    simulations.Add(vehicle.Id);
                }
            }

            if (wasBrandNewPlayer)
            {
                SetupPlayerEntity(player);
            }
            else
            {
                RespawnExistingEntity(player);
            }

            InitialPlayerSync initialPlayerSync = new(player.GameObjectId,
                wasBrandNewPlayer,
                assignedEscapePodId,
                equippedItems,
                GetAllModules(world.InventoryManager.GetAllModules(), player.GetModules()),
                world.BaseManager.GetBasePiecesForNewlyConnectedPlayer(),
                vehicles,
                world.InventoryManager.GetAllStorageSlotItems(),
                player.UsedItems,
                player.QuickSlotsBinding,
                world.GameData.PDAState.GetInitialPDAData(),
                world.GameData.StoryGoals.GetInitialStoryGoalData(scheduleKeeper),
                player.CompletedGoals,
                player.Position,
                player.Rotation,
                player.SubRootId,
                player.Stats,
                GetOtherPlayers(player),
                world.WorldEntityManager.GetGlobalRootEntities(),
                simulations,
                world.GameMode,
                player.Permissions,
                player.PingInstancePreferences.ToDictionary(m => m.Key, m => m.Value)
            );

            player.SendPacket(new TimeChange(eventTriggerer.ElapsedSeconds, true));
            player.SendPacket(initialPlayerSync);
        }

        private IEnumerable<PlayerContext> GetOtherPlayers(Player player)
        {
            return playerManager.GetConnectedPlayers().Where(p => p != player)
                                                      .Select(p => p.PlayerContext);
        }

        private List<EquippedItemData> GetAllModules(ICollection<EquippedItemData> globalModules, List<EquippedItemData> playerModules)
        {
            List<EquippedItemData> modulesToSync = new();
            modulesToSync.AddRange(globalModules);
            modulesToSync.AddRange(playerModules);
            return modulesToSync;
        }

        private void SetupPlayerEntity(Player player)
        {
            NitroxTransform transform = new(player.Position, player.Rotation, NitroxVector3.One);

            PlayerWorldEntity playerEntity = new PlayerWorldEntity(transform, 0, null, false, null, true, player.GameObjectId, NitroxTechType.None, null, null, new List<Entity>());
            entityRegistry.AddEntity(playerEntity);
            world.WorldEntityManager.TrackEntityInTheWorld(playerEntity);
            playerManager.SendPacketToOtherPlayers(new CellEntities(playerEntity), player);
        }

        private void RespawnExistingEntity(Player player)
        {
            Optional<Entity> playerEntity = entityRegistry.GetEntityById(player.PlayerContext.PlayerNitroxId);

            if (playerEntity.HasValue)
            {
                playerManager.SendPacketToOtherPlayers(new CellEntities(playerEntity.Value, true), player);
            }
            else
            {
                Log.Error($"Unable to find player entity for {player.Name}");
            }
        }
    }
}

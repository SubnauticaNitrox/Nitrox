using System.Collections.Generic;
using System.Linq;
using System.Net;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.Serialization.World;

namespace NitroxServer.Communication.Packets.Processors
{
    public class PlayerJoiningMultiplayerSessionProcessor : UnauthenticatedPacketProcessor<PlayerJoiningMultiplayerSession>
    {
        private readonly PlayerManager playerManager;
        private readonly ScheduleKeeper scheduleKeeper;
        private readonly EventTriggerer eventTriggerer;
        private readonly World world;

        public PlayerJoiningMultiplayerSessionProcessor(ScheduleKeeper scheduleKeeper, EventTriggerer eventTriggerer, PlayerManager playerManager, World world)
        {
            this.scheduleKeeper = scheduleKeeper;
            this.eventTriggerer = eventTriggerer;
            this.playerManager = playerManager;
            this.world = world;
        }

        public override void Process(PlayerJoiningMultiplayerSession packet, NitroxConnection connection)
        {
            Player player = playerManager.PlayerConnected(connection, packet.ReservationKey, out bool wasBrandNewPlayer);

            NitroxId assignedEscapePodId = world.EscapePodManager.AssignPlayerToEscapePod(player.Id, out Optional<EscapePodModel> newlyCreatedEscapePod);
            if (newlyCreatedEscapePod.HasValue)
            {
                AddEscapePod addEscapePod = new(newlyCreatedEscapePod.Value);
                playerManager.SendPacketToOtherPlayers(addEscapePod, player);
            }

            List<EquippedItemData> equippedItems = player.GetEquipment();
            List<NitroxTechType> techTypes = equippedItems.Select(equippedItem => equippedItem.TechType).ToList();
            List<ItemData> inventoryItems = GetInventoryItems(player.GameObjectId);

            PlayerJoinedMultiplayerSession playerJoinedPacket = new(player.PlayerContext, player.SubRootId, techTypes, inventoryItems);
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

            InitialPlayerSync initialPlayerSync = new(player.GameObjectId,
                wasBrandNewPlayer,
                world.EscapePodManager.GetEscapePods(),
                assignedEscapePodId,
                equippedItems,
                GetAllModules(world.InventoryManager.GetAllModules(), player.GetModules()),
                world.BaseManager.GetBasePiecesForNewlyConnectedPlayer(),
                vehicles,
                world.InventoryManager.GetAllInventoryItems(),
                world.InventoryManager.GetAllStorageSlotItems(),
                player.UsedItems,
                player.QuickSlotsBinding,
                world.GameData.PDAState.GetInitialPDAData(),
                world.GameData.StoryGoals.GetInitialStoryGoalData(scheduleKeeper),
                new HashSet<string>(player.CompletedGoals),
                player.Position,
                player.SubRootId,
                player.Stats,
                GetRemotePlayerData(player),
                world.EntityManager.GetGlobalRootEntities(),
                simulations,
                world.GameMode,
                player.Permissions,
                player.PingInstancePreferences.ToDictionary(m => m.Key, m => m.Value)
            );

            player.SendPacket(new TimeChange(eventTriggerer.ElapsedSeconds, true));
            player.SendPacket(initialPlayerSync);
        }

        private List<InitialRemotePlayerData> GetRemotePlayerData(Player player)
        {
            List<InitialRemotePlayerData> playerData = new();

            foreach (Player otherPlayer in playerManager.GetConnectedPlayers())
            {
                if (!player.Equals(otherPlayer))
                {
                    List<EquippedItemData> equippedItems = otherPlayer.GetEquipment();
                    List<NitroxTechType> techTypes = equippedItems.Select(equippedItem => equippedItem.TechType).ToList();

                    InitialRemotePlayerData remotePlayer = new(otherPlayer.PlayerContext, otherPlayer.Position, otherPlayer.SubRootId, techTypes);
                    playerData.Add(remotePlayer);
                }
            }

            return playerData;
        }

        private List<EquippedItemData> GetAllModules(ICollection<EquippedItemData> globalModules, List<EquippedItemData> playerModules)
        {
            List<EquippedItemData> modulesToSync = new();
            modulesToSync.AddRange(globalModules);
            modulesToSync.AddRange(playerModules);
            return modulesToSync;
        }

        private List<ItemData> GetInventoryItems(NitroxId playerID)
        {
            List<ItemData> inventoryItems = world.InventoryManager.GetAllInventoryItems().Where(item => item.ContainerId.Equals(playerID)).ToList();

            for (int index = 0; index < inventoryItems.Count; index++) //Also add batteries from tools to inventory items.
            {
                inventoryItems.AddRange(world.InventoryManager.GetAllStorageSlotItems().Where(item => item.ContainerId.Equals(inventoryItems[index].ItemId)));
            }

            return inventoryItems;
        }
    }
}

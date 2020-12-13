using System.Collections.Generic;
using System.Linq;
using System.Net;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Server.Communication.NetworkingLayer;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.Serialization.World;

namespace Nitrox.Server.Communication.Packets.Processors
{
    public class PlayerJoiningMultiplayerSessionProcessor : UnauthenticatedPacketProcessor<PlayerJoiningMultiplayerSession>
    {
        private readonly PlayerManager playerManager;
        private readonly TimeKeeper timeKeeper;
        private readonly World world;

        public PlayerJoiningMultiplayerSessionProcessor(TimeKeeper timeKeeper,
            PlayerManager playerManager, World world)
        {
            this.timeKeeper = timeKeeper;
            this.playerManager = playerManager;
            this.world = world;
        }

        public override void Process(PlayerJoiningMultiplayerSession packet, NitroxConnection connection)
        {
            bool wasBrandNewPlayer;
            Player player = playerManager.PlayerConnected(connection, packet.ReservationKey, out wasBrandNewPlayer);
            timeKeeper.SendCurrentTimePacket(player);

            Optional<EscapePodModel> newlyCreatedEscapePod;
            NitroxId assignedEscapePodId = world.EscapePodManager.AssignPlayerToEscapePod(player.Id, out newlyCreatedEscapePod);
            if (newlyCreatedEscapePod.HasValue)
            {
                AddEscapePod addEscapePod = new AddEscapePod(newlyCreatedEscapePod.Value);
                playerManager.SendPacketToOtherPlayers(addEscapePod, player);
            }

            List<EquippedItemData> equippedItems = player.GetEquipment();
            List<NitroxTechType> techTypes = equippedItems.Select(equippedItem => equippedItem.TechType).ToList();

            PlayerJoinedMultiplayerSession playerJoinedPacket = new PlayerJoinedMultiplayerSession(player.PlayerContext, techTypes);
            playerManager.SendPacketToOtherPlayers(playerJoinedPacket, player);

            // Make players on localhost admin by default.
            if (IPAddress.IsLoopback(connection.Endpoint.Address))
            {
                player.Permissions = Perms.ADMIN;
            }

            List<NitroxId> simulations = world.EntitySimulation.AssignGlobalRootEntities(player).ToList();
            IEnumerable<VehicleModel> vehicles = world.VehicleManager.GetVehicles();
            foreach(VehicleModel vehicle in vehicles)
            {
                if (world.SimulationOwnershipData.TryToAcquire(vehicle.Id, player, SimulationLockType.TRANSIENT))
                {
                    simulations.Add(vehicle.Id);
                }
            }

            InitialPlayerSync initialPlayerSync = new InitialPlayerSync(player.GameObjectId,
                wasBrandNewPlayer,
                world.EscapePodManager.GetEscapePods(),
                assignedEscapePodId,
                equippedItems,
                player.GetModules(),
                world.BaseManager.GetBasePiecesForNewlyConnectedPlayer(),
                vehicles,
                world.InventoryManager.GetAllInventoryItems(),
                world.InventoryManager.GetAllStorageSlotItems(),
                world.GameData.PDAState.GetInitialPDAData(),
                world.GameData.StoryGoals.GetInitialStoryGoalData(),
                player.Position,
                player.SubRootId,
                player.Stats,
                GetRemotePlayerData(player),
                world.EntityManager.GetGlobalRootEntities(),
                simulations,
                world.GameMode,
                player.Permissions);

            player.SendPacket(initialPlayerSync);
        }

        private List<InitialRemotePlayerData> GetRemotePlayerData(Player player)
        {
            List<InitialRemotePlayerData> playerData = new List<InitialRemotePlayerData>();

            foreach (Player otherPlayer in playerManager.GetConnectedPlayers())
            {
                if (!player.Equals(otherPlayer))
                {
                    List<EquippedItemData> equippedItems = otherPlayer.GetEquipment();
                    List<NitroxTechType> techTypes = equippedItems.Select(equippedItem => equippedItem.TechType).ToList();

                    InitialRemotePlayerData remotePlayer = new InitialRemotePlayerData(otherPlayer.PlayerContext, otherPlayer.Position, otherPlayer.SubRootId, techTypes);
                    playerData.Add(remotePlayer);
                }
            }

            return playerData;
        }
    }
}

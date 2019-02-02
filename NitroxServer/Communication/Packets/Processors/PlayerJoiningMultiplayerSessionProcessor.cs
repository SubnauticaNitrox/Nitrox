using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.Serialization.World;
using NitroxModel.Logger;

namespace NitroxServer.Communication.Packets.Processors
{
    public class PlayerJoiningMultiplayerSessionProcessor : UnauthenticatedPacketProcessor<PlayerJoiningMultiplayerSession>
    {
        private readonly TimeKeeper timeKeeper;
        private readonly PlayerManager playerManager;
        private readonly World world;

        public PlayerJoiningMultiplayerSessionProcessor(TimeKeeper timeKeeper,
            PlayerManager playerManager, World world)
        {
            this.timeKeeper = timeKeeper;
            this.playerManager = playerManager;
            this.world = world;
        }

        public override void Process(PlayerJoiningMultiplayerSession packet, Connection connection)
        {
            bool wasBrandNewPlayer;
            Player player = playerManager.CreatePlayer(connection, packet.ReservationKey, out wasBrandNewPlayer);
            timeKeeper.SendCurrentTimePacket(player);

            Optional<EscapePodModel> newlyCreatedEscapePod;
            string assignedEscapePodGuid = world.EscapePodManager.AssignPlayerToEscapePod(player.Id, out newlyCreatedEscapePod);
            if(newlyCreatedEscapePod.IsPresent())
            {
                AddEscapePod addEscapePod = new AddEscapePod(newlyCreatedEscapePod.Get());
                playerManager.SendPacketToOtherPlayers(addEscapePod, player);
            }

            List<EquippedItemData> equippedItems = world.PlayerData.GetEquippedItemsForInitialSync(player.Name);
            List<TechType> techTypes = equippedItems.Select(equippedItem => equippedItem.TechType).ToList();

            PlayerJoinedMultiplayerSession playerJoinedPacket = new PlayerJoinedMultiplayerSession(player.PlayerContext, techTypes);
            playerManager.SendPacketToOtherPlayers(playerJoinedPacket, player);

            Log.Info("{0} joined!", player.Name); // This is nice to have

            InitialPlayerSync initialPlayerSync = new InitialPlayerSync(player.Id.ToString(),
                                                                       wasBrandNewPlayer,
                                                                       world.EscapePodData.EscapePods,
                                                                       assignedEscapePodGuid,
                                                                       equippedItems,
                                                                       world.BaseData.GetBasePiecesForNewlyConnectedPlayer(),
                                                                       world.VehicleData.GetVehiclesForInitialSync(),
                                                                       world.InventoryData.GetAllItemsForInitialSync(),
                                                                       world.GameData.PDAState.GetInitialPdaData(),
                                                                       world.PlayerData.GetPlayerSpawn(player.Name),
                                                                       world.PlayerData.GetSubRootGuid(player.Name),
                                                                       world.PlayerData.GetPlayerStats(player.Name),
                                                                       getRemotePlayerData(player),
                                                                       world.EntityData.GetGlobalRootEntities(),
                                                                       world.GameMode,
                                                                       world.PlayerData.GetPermissions(player.Name));

            player.SendPacket(initialPlayerSync);
        }

        private List<InitialRemotePlayerData> getRemotePlayerData(Player player)
        {
            List<InitialRemotePlayerData> playerData = new List<InitialRemotePlayerData>();

            foreach (Player otherPlayer in playerManager.GetPlayers())
            {
                if (!player.Equals(otherPlayer))
                {
                    List<EquippedItemData> equippedItems = world.PlayerData.GetEquippedItemsForInitialSync(otherPlayer.Name);
                    List<TechType> techTypes = equippedItems.Select(equippedItem => equippedItem.TechType).ToList();

                    InitialRemotePlayerData remotePlayer = new InitialRemotePlayerData(otherPlayer.PlayerContext, otherPlayer.Position, otherPlayer.SubRootGuid, techTypes);
                    playerData.Add(remotePlayer);
                }
            }

            return playerData;
        }
    }
}

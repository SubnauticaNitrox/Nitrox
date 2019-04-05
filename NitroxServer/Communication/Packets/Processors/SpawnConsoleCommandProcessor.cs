using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;
using UnityEngine;

namespace NitroxServer.Communication.Packets.Processors
{
    public class SpawnConsoleCommandProcessor : AuthenticatedPacketProcessor<SpawnConsoleCommandEvent>
    {
        private readonly PlayerManager playerManager;
        VehicleData vehicleData;

        public SpawnConsoleCommandProcessor(PlayerManager playerManager, VehicleData vehicleData)
        {
            this.playerManager = playerManager;
            this.vehicleData = vehicleData;
        }

        public override void Process(SpawnConsoleCommandEvent packet, Player player)
        {

            vehicleData.AddVehicle(packet.Vehiclemodel);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

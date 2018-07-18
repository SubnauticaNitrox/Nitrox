using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer.Communication.Packets.Processors
{
    class VehicleRemovePacketProcessor : AuthenticatedPacketProcessor<VehicleDestroyed>
    {
        private readonly PlayerManager playerManager;
        private readonly VehicleData vehicleData;

        public VehicleRemovePacketProcessor(PlayerManager playerManager, VehicleData vehicleData)
        {
            this.playerManager = playerManager;
            this.vehicleData = vehicleData;
        }

        public override void Process(VehicleDestroyed packet, Player player)
        {
            NitroxModel.Logger.Log.Info(packet.ToString());
            vehicleData.RemoveVehicle(packet.Guid);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

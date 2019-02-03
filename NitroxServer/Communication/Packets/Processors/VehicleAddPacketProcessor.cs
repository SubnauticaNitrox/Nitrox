using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer.Communication.Packets.Processors
{
    class VehicleAddPacketProcessor : AuthenticatedPacketProcessor<VehicleCreated>
    {
        private readonly PlayerManager playerManager;
        private readonly VehicleData vehicleData;

        public VehicleAddPacketProcessor(PlayerManager playerManager, VehicleData vehicleData)
        {
            this.playerManager = playerManager;
            this.vehicleData = vehicleData;
        }

        public override void Process(VehicleCreated packet, Player player)
        {
            Log.Info(packet);
            if(packet.CreatedVehicle.TechType != TechType.Exosuit)
            {
                vehicleData.AddVehicle(packet.CreatedVehicle);
            }
            else
            {
                vehicleData.AddExosuit(packet.CreatedExosuit);
            }
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer.Communication.Packets.Processors
{
    class VehicleColorChangeProcessor : AuthenticatedPacketProcessor<VehicleColorChange>
    {
        private readonly PlayerManager playerManager;
        private readonly VehicleData vehicleData;

        public VehicleColorChangeProcessor(PlayerManager playerManager, VehicleData vehicleData)
        {
            this.playerManager = playerManager;
            this.vehicleData = vehicleData;
        }

        public override void Process(VehicleColorChange packet, Player player)
        {
            vehicleData.UpdateVehicleColours(packet.Index, packet.Id, packet.HSB, packet.Color);
            playerManager.SendPacketToOtherPlayers(packet, player);

            Log2.Instance.Log(NLogType.Info, $"{packet}");
        }
    }
}

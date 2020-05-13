using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer_Subnautica.Communication.Packets.Processors
{
    class RocketToggleElevatorProcessor : AuthenticatedPacketProcessor<RocketToggleElevator>
    {
        private readonly VehicleManager vehicleManager;
        private readonly PlayerManager playerManager;

        public RocketToggleElevatorProcessor(VehicleManager vehicleManager, PlayerManager playerManager)
        {
            this.vehicleManager = vehicleManager;
            this.playerManager = playerManager;
        }

        public override void Process(RocketToggleElevator packet, NitroxServer.Player player)
        {
            Optional<NeptuneRocketModel> opRocket = vehicleManager.GetVehicleModel<NeptuneRocketModel>(packet.Id);

            if (opRocket.HasValue && opRocket.Value.GetType() == typeof(NeptuneRocketModel))
            {
                opRocket.Value.ElevatorUp = packet.Up;
            }

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

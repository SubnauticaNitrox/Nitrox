using Nitrox.Model.DataStructures;
using Nitrox.Model.Logger;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Vehicles;

namespace Nitrox.Server.Subnautica.Communication.Packets.Processors
{
    public class RocketPreflightCompleteProcessor : AuthenticatedPacketProcessor<RocketPreflightComplete>
    {
        private readonly VehicleManager vehicleManager;
        private readonly PlayerManager playerManager;

        public RocketPreflightCompleteProcessor(VehicleManager vehicleManager, PlayerManager playerManager)
        {
            this.vehicleManager = vehicleManager;
            this.playerManager = playerManager;
        }

        public override void Process(RocketPreflightComplete packet, Player player)
        {
            Optional<NeptuneRocketModel> opRocket = vehicleManager.GetVehicleModel<NeptuneRocketModel>(packet.Id);

            if (opRocket.HasValue)
            {
                opRocket.Value.PreflightChecks.Add(packet.FlightCheck);
            }
            else
            {
                Log.Error($"{nameof(RocketPreflightCompleteProcessor)}: Can't find server model for rocket with id {packet.Id}");
            }

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer_Subnautica.Communication.Packets.Processors
{
    public class RocketStageUpdateProcessor : AuthenticatedPacketProcessor<RocketStageUpdate>
    {
        private readonly VehicleManager vehicleManager;
        private readonly PlayerManager playerManager;

        public RocketStageUpdateProcessor(VehicleManager vehicleManager, PlayerManager playerManager)
        {
            this.vehicleManager = vehicleManager;
            this.playerManager = playerManager;
        }

        public override void Process(RocketStageUpdate packet, NitroxServer.Player player)
        {
            Optional<NeptuneRocketModel> opRocket = vehicleManager.GetVehicleModel<NeptuneRocketModel>(packet.Id);

            if (opRocket.HasValue)
            {
                /* Rocket building states :
                 * 0 : Neptune Launch Platform
                 * 1 : Neptune Gantry
                 * 2 : Neptune Boosters
                 * 3 : Neptune Fuel Reserve
                 * 4 : Neptune Cockpit
                 * 
                 * A finished rocket will be in the state 5 which cannot be reached for the server (only) based on players events, so we do it by hand
                 */
                opRocket.Value.CurrentStage = packet.NewStage == 4 ? 5 : packet.NewStage;
            }
            else
            {
                Log.Error($"{nameof(RocketStageUpdateProcessor)}: Can't find server model for rocket with id {packet.Id}");
            }

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

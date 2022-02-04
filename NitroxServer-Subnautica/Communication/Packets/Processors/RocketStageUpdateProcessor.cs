using NitroxModel.DataStructures.Util;
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
                if (packet.NewStage > opRocket.Value.CurrentStage)
                {
                    opRocket.Value.CurrentStage = packet.NewStage;
                }
                else
                {
                    Log.Error($"{nameof(RocketStageUpdateProcessor)}: Received invalid data to update existing '{packet.Id}' rocket stage (Received : {packet.NewStage}, Expected : {opRocket.Value.CurrentStage + 1})");
                    //TODO : Handle desync by overriding the stage and reconstruct the rocket client side
                }
            }
            else
            {
                Log.Error($"{nameof(RocketStageUpdateProcessor)}: Can't find server model for rocket with id {packet.Id}");
            }

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

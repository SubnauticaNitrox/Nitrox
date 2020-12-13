using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Vehicles;

namespace Nitrox.Server.Subnautica.Communication.Packets.Processors
{
    class ToggleLightsProcessor : AuthenticatedPacketProcessor<Model.Packets.ToggleLights>
    {
        private readonly VehicleManager vehicleManager;
        private readonly PlayerManager playerManager;

        public ToggleLightsProcessor(VehicleManager vehicleManager, PlayerManager playerManager)
        {
            this.vehicleManager = vehicleManager;
            this.playerManager = playerManager;
        }        

        public override void Process(Model.Packets.ToggleLights packet, Player player)
        {
            Optional<SeamothModel> opSeamoth = vehicleManager.GetVehicleModel<SeamothModel>(packet.Id);

            if (opSeamoth.HasValue && opSeamoth.Value.GetType() == typeof(SeamothModel))
            {
                opSeamoth.Value.LightOn = packet.IsOn;
            }

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

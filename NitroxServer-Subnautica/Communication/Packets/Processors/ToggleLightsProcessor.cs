using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer_Subnautica.Communication.Packets.Processors
{
    class ToggleLightsProcessor : AuthenticatedPacketProcessor<NitroxModel.Packets.ToggleLights>
    {
        private readonly VehicleManager vehicleManager;
        private readonly PlayerManager playerManager;

        public ToggleLightsProcessor(VehicleManager vehicleManager, PlayerManager playerManager)
        {
            this.vehicleManager = vehicleManager;
            this.playerManager = playerManager;
        }        

        public override void Process(NitroxModel.Packets.ToggleLights packet, NitroxServer.Player player)
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

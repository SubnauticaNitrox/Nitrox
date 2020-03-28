using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer_Subnautica.Communication.Packets.Processors
{
    class CyclopsToggleInternalLightingProcessor : AuthenticatedPacketProcessor<CyclopsToggleInternalLighting>
    {
        private readonly VehicleManager vehicleManager;
        private readonly PlayerManager playerManager;

        public CyclopsToggleInternalLightingProcessor(VehicleManager vehicleManager, PlayerManager playerManager)
        {
            this.vehicleManager = vehicleManager;
            this.playerManager = playerManager;
        }

        public override void Process(CyclopsToggleInternalLighting packet, NitroxServer.Player player)
        {
            Optional<CyclopsModel> opCyclops = vehicleManager.GetVehicleModel<CyclopsModel>(packet.Id);
            if (opCyclops.HasValue && opCyclops.Value.GetType() == typeof(CyclopsModel))
            {
                opCyclops.Value.InternalLightsOn = packet.IsOn;
            }

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

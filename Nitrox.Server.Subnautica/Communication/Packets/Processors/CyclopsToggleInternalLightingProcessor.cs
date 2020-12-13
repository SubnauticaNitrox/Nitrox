using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Vehicles;

namespace Nitrox.Server.Subnautica.Communication.Packets.Processors
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

        public override void Process(CyclopsToggleInternalLighting packet, Player player)
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

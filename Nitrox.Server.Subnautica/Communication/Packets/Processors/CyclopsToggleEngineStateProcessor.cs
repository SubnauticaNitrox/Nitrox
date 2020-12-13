using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Vehicles;

namespace Nitrox.Server.Subnautica.Communication.Packets.Processors
{
    class CyclopsToggleEngineStateProcessor : AuthenticatedPacketProcessor<CyclopsToggleEngineState>
    {
        private readonly VehicleManager vehicleManager;
        private readonly PlayerManager playerManager;

        public CyclopsToggleEngineStateProcessor(VehicleManager vehicleManager, PlayerManager playerManager)
        {
            this.vehicleManager = vehicleManager;
            this.playerManager = playerManager;
        }

        public override void Process(CyclopsToggleEngineState packet, Player player)
        {
            Optional<CyclopsModel> opCyclops = vehicleManager.GetVehicleModel<CyclopsModel>(packet.Id);
            if (opCyclops.HasValue)
            {
                // If someone starts the engine, IsOn will be false, so only isStarting contains the info we need
                opCyclops.Value.EngineState = packet.IsStarting;
            }

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

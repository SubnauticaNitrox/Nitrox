using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer_Subnautica.Communication.Packets.Processors
{
    class CyclopsChangeSilentRunningProcessor : AuthenticatedPacketProcessor<CyclopsChangeSilentRunning>
    {
        private readonly VehicleManager vehicleManager;
        private readonly PlayerManager playerManager;

        public CyclopsChangeSilentRunningProcessor(VehicleManager vehicleManager, PlayerManager playerManager)
        {
            this.vehicleManager = vehicleManager;
            this.playerManager = playerManager;
        }

        public override void Process(CyclopsChangeSilentRunning packet, NitroxServer.GameLogic.Player player)
        {
            Optional<CyclopsModel> opCyclops = vehicleManager.GetVehicleModel<CyclopsModel>(packet.Id);
            if (opCyclops.HasValue)
            {
                opCyclops.Value.SilentRunningOn = packet.IsOn;
            }

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

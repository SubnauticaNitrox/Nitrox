using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer.Communication.Packets.Processors
{
    class LiveMixinChangedProcessor : AuthenticatedPacketProcessor<LiveMixinHealthChanged>
    {
        private readonly PlayerManager playerManager;
        private readonly VehicleManager vehicleManager;

        public LiveMixinChangedProcessor(PlayerManager playerManager, VehicleManager vehicleManager)
        {
            this.playerManager = playerManager;
            this.vehicleManager = vehicleManager;
        }

        public override void Process(LiveMixinHealthChanged packet, Player player)
        {
            // If no vehicle exists with the id, we wont do anything anyways, therefore we do not care for that case (at the moment)
            Log.Debug($"Health changed packet recieved for {packet.Id} (Health changed: {packet.LifeChanged}, Total Health: {packet.TotalHealth}");
            vehicleManager.UpdateVehicleHealth(packet.Id, packet.TotalHealth);

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

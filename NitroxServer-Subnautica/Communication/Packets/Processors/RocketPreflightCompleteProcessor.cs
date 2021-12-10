using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer_Subnautica.Communication.Packets.Processors
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

        public override void Process(RocketPreflightComplete packet, NitroxServer.Player player)
        {
            Optional<NeptuneRocketModel> opRocket = vehicleManager.GetVehicleModel<NeptuneRocketModel>(packet.Id);

            if (opRocket.HasValue)
            {
                ThreadSafeList<PreflightCheck> list = opRocket.Value.PreflightChecks;

                if (!list.Contains(packet.FlightCheck))
                {
                    list.Add(packet.FlightCheck);
                }
                else
                {
                    Log.Error($"{nameof(RocketPreflightCompleteProcessor)}: Received an existing preflight '{packet.FlightCheck}' for rocket '{packet.Id}'");
                }

            }
            else
            {
                Log.Error($"{nameof(RocketPreflightCompleteProcessor)}: Can't find server model for rocket with id {packet.Id}");
            }

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

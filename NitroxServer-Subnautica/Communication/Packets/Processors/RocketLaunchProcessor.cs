using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer_Subnautica.Communication.Packets.Processors;

public class RocketLaunchProcessor : AuthenticatedPacketProcessor<RocketLaunch>
{
    private readonly VehicleManager vehicleManager;
    private readonly PlayerManager playerManager;

    public RocketLaunchProcessor(VehicleManager vehicleManager, PlayerManager playerManager)
    {
        this.vehicleManager = vehicleManager;
        this.playerManager = playerManager;
    }

    public override void Process(RocketLaunch packet, NitroxServer.Player player)
    {
        Optional<NeptuneRocketModel> opRocket = vehicleManager.GetVehicleModel<NeptuneRocketModel>(packet.RocketId);

        if (opRocket.HasValue)
        {
            // 5 is the value of RocketPreflightCheckManager.totalPreflightChecks
            if (opRocket.Value.PreflightChecks.Count == 5)
            {
                // Tell every player to launch rocket
                playerManager.SendPacketToAllPlayers(packet);
            }
            else
            {
                // Resync all players' preflight checks
                playerManager.SendPacketToAllPlayers(new RocketResync(packet.RocketId, opRocket.Value.PreflightChecks.ToList()));
            }
        }
        else
        {
            Log.Error($"{nameof(RocketLaunchProcessor)}: Can't find server model for rocket with id {packet.RocketId}");
        }
    }
}

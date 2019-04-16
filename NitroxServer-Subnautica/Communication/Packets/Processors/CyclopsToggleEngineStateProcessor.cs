using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.Packets;
using NitroxServer;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer_Subnautica.Communication.Packets.Processors
{
    class CyclopsToggleEngineStateProcessor : AuthenticatedPacketProcessor<CyclopsToggleEngineState>
    {
        public VehicleData Vehicles { get; }
        public PlayerManager PlayerManager { get; }

        public CyclopsToggleEngineStateProcessor(VehicleData vehicleData, PlayerManager playerManager)
        {
            Vehicles = vehicleData;
            PlayerManager = playerManager;
        }

        public override void Process(CyclopsToggleEngineState packet, NitroxServer.Player player)
        {
            Optional<CyclopsModel> opCyclops = Vehicles.GetVehicleModel<CyclopsModel>(packet.Id);
            if (opCyclops.IsPresent())
            {
                // If someone starts the engine, IsOn will be false, so only isStarting contains the info we need
                opCyclops.Get().EngineState = packet.IsStarting;
            }
            PlayerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxServer;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer_Subnautica.Communication.Packets.Processors
{
    class ToggleLightsProcessor : AuthenticatedPacketProcessor<NitroxModel.Packets.ToggleLights>
    {
        public VehicleData Vehicles { get; }
        public PlayerManager PlayerManager { get; }

        public ToggleLightsProcessor(VehicleData vehicleData, PlayerManager playerManager)
        {
            Vehicles = vehicleData;
            PlayerManager = playerManager;
        }        

        public override void Process(NitroxModel.Packets.ToggleLights packet, NitroxServer.Player player)
        {
            Optional<SeamothModel> opSeamoth = Vehicles.GetVehicleModel<SeamothModel>(packet.Id);
            if (opSeamoth.IsPresent() && opSeamoth.Get().GetType() == typeof(SeamothModel))
            {
                opSeamoth.Get().LightOn = packet.IsOn;
            }
            PlayerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxServer;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer_Subnautica.Communication.Packets.Processors
{
    class ToggleLightsProcessor : AuthenticatedPacketProcessor<NitroxModel.Packets.ToggleLights>
    {
        public VehicleData Vehicles { get; }

        public ToggleLightsProcessor(VehicleData vehicleData)
        {
            Vehicles = vehicleData;
        }        

        public override void Process(NitroxModel.Packets.ToggleLights packet, NitroxServer.Player player)
        {
            Optional<SeamothModel> opSeamoth = Vehicles.GetVehicleModel<SeamothModel>(packet.Guid);
            if (opSeamoth.IsPresent() && opSeamoth.Get().GetType() == typeof(SeamothModel))
            {
                NitroxModel.Logger.Log.Debug("Set seamoth lights to " + packet.IsOn + " for seamoth " + packet.Guid);
                opSeamoth.Get().LightOn = packet.IsOn;
            }
        }
    }
}

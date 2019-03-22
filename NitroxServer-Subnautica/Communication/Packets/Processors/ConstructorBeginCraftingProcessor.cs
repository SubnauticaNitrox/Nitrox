using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer_Subnautica.Communication.Packets.Processors
{
    class ConstructorBeginCraftingProcessor : AuthenticatedPacketProcessor<ConstructorBeginCrafting>
    {
        private readonly PlayerManager playerManager;
        private readonly VehicleData vehicleData;

        public ConstructorBeginCraftingProcessor(PlayerManager playerManager, VehicleData vehicleData)
        {
            this.playerManager = playerManager;
            this.vehicleData = vehicleData;
        }

        public override void Process(ConstructorBeginCrafting packet, NitroxServer.Player player)
        {
            VehicleModel vehicleModel = VehicleModelFactory.BuildFrom(packet);
            vehicleData.AddVehicle(vehicleModel);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer.Communication.Packets.Processors
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

        public override void Process(ConstructorBeginCrafting packet, Player player)
        {
            VehicleModel vehicleModel = VehicleModelFactory.BuildFrom(packet);
            vehicleData.AddVehicle(vehicleModel);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

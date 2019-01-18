using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
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
            vehicleData.AddVehicle(new VehicleModel(packet.TechType, packet.ConstructedItemGuid, packet.Position, packet.Rotation,Optional<List<InteractiveChildObjectIdentifier>>.OfNullable(packet.InteractiveChildIdentifiers), Optional<string>.Empty()));
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

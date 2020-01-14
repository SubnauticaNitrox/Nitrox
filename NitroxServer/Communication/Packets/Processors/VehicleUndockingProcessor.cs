﻿using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer.Communication.Packets.Processors
{
    class VehicleUndockingProcessor : AuthenticatedPacketProcessor<VehicleUndocking>
    {
        private readonly PlayerManager playerManager;
        private readonly VehicleData vehicleData;

        public VehicleUndockingProcessor(PlayerManager playerManager, VehicleData vehicleData)
        {
            this.playerManager = playerManager;
            this.vehicleData = vehicleData;
        }

        public override void Process(VehicleUndocking packet, Player player)
        {
            Optional<VehicleModel> vehicle = vehicleData.GetVehicleModel(packet.VehicleId);
            if (!vehicle.IsPresent())
            {
                return;
            }

            VehicleModel vehicleModel = vehicle.Get();
            vehicleModel.DockingBayId = Optional<NitroxId>.Empty();

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

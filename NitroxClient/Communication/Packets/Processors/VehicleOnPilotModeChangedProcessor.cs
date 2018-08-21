﻿using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleOnPilotModeChangedProcessor : ClientPacketProcessor<VehicleOnPilotModeChanged>
    {
        private readonly IPacketSender packetSender;
        private readonly Vehicles vehicles;

        public VehicleOnPilotModeChangedProcessor(IPacketSender packetSender, Vehicles vehicles)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
        }

        public override void Process(VehicleOnPilotModeChanged packet)
        {
            GameObject vehicleGo = GuidHelper.RequireObjectFrom(packet.VehicleGuid);
            Vehicle vehicle = vehicleGo.RequireComponent<Vehicle>();
            
            // If the vehicle is docked, then we will manually set the piloting mode
            // once the animations complete.  This prevents weird behaviour such as the
            // player existing the vehicle while it is about to dock (the event fires 
            // before the animation completes on the remote player.)
            if (!vehicle.docked)
            {
                vehicles.SetOnPilotMode(packet.VehicleGuid, packet.PlayerId, packet.IsPiloting);
            }
        }
    }
}

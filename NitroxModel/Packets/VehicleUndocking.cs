﻿using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleUndocking : Packet
    {
        public NitroxId VehicleId { get; }
        public NitroxId DockId { get; }
        public ushort PlayerId { get; }

        public VehicleUndocking(NitroxId vehicleId, NitroxId dockId, ushort playerId)
        {
            VehicleId = vehicleId;
            DockId = dockId;
            PlayerId = playerId;
        }

        public override string ToString()
        {
            return "[VehicleUndocking VehicleId: " + VehicleId + " DockId: " + DockId + " PlayerId: " + PlayerId + "]";
        }
    }
}

﻿using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleUndocking : Packet
    {
        public string VehicleGuid { get; }
        public string DockGuid { get; }
        public ushort PlayerId { get; }

        public VehicleUndocking(string vehicleGuid, string dockGuid, ushort playerId)
        {
            VehicleGuid = vehicleGuid;
            DockGuid = dockGuid;
            PlayerId = playerId;
        }

        public override string ToString()
        {
            return "[VehicleUndocking VehicleGuid: " + VehicleGuid + " DockGuid: " + DockGuid + " PlayerId: " + PlayerId + "]";
        }
    }
}

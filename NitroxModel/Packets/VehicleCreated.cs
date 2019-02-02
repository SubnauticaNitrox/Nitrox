using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleCreated : Packet
    {
        public string PlayerName { get; }
        public VehicleModel CreatedVehicle { get; }

        public VehicleCreated(VehicleModel createdVehicle, string playerName)
        {
            CreatedVehicle = createdVehicle;
            PlayerName = playerName;
        }

        public override string ToString()
        {
            return "[VehicleCreated - Vehicle Guid: " + CreatedVehicle.Guid + " PlayerName: " + PlayerName + " Vehicle Type: " + CreatedVehicle.TechType + "]";
        }
    }
}

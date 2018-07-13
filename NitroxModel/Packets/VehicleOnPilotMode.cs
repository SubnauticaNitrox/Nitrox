using System;
using NitroxModel.DataStructures.Util;
using UnityEngine;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleOnPilotMode : Packet
    {
        public string VehicleGuid { get; }
        public string PlayerGuid { get; }
        public byte Type { get; }

        public VehicleOnPilotMode(string vehicleGuid, string playerGuid,byte type)
        {
            VehicleGuid = vehicleGuid;
            PlayerGuid = playerGuid;
            Type = type;
        }

        public override string ToString()
        {
            return "[VehicleOnPilotMode - VehicleGuid: " + VehicleGuid + " PlayerGuid: " + PlayerGuid + " Type: " + Type + "]";
        }
    }
}

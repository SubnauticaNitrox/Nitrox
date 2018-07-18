using System;
using NitroxModel.DataStructures.Util;
using UnityEngine;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleOnPilotModeChanged : Packet
    {
        public string VehicleGuid { get; }
        public string PlayerGuid { get; }
        public bool Type { get; }

        public VehicleOnPilotModeChanged(string vehicleGuid, string playerGuid,bool type)
        {
            VehicleGuid = vehicleGuid;
            PlayerGuid = playerGuid;
            Type = type;
        }

        public override string ToString()
        {
            return "[VehicleOnPilotModeChanged - VehicleGuid: " + VehicleGuid + " PlayerGuid: " + PlayerGuid + " Type: " + Type + "]";
        }
    }
}

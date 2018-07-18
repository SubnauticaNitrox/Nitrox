using System;
using NitroxModel.DataStructures.Util;
using UnityEngine;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleCreated : VehicleMovement
    {
        public VehicleModel VehicleData { get; }

        public VehicleCreated(VehicleModel vehicle) : base(string.Empty, vehicle)
        {
            VehicleData = vehicle;
        }

        public override string ToString()
        {
            return "[VehicleCreated - Vehicle: " + Vehicle +  "]\n\t" + base.ToString();
        }
    }
}

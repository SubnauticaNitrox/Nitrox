using System;
using NitroxModel.DataStructures.Util;
using UnityEngine;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleAddEntry : VehicleMovement
    {
        public VehicleModel VehicleData { get; }

        public VehicleAddEntry(VehicleModel vehicle) : base(string.Empty, vehicle)
        {
            VehicleData = vehicle;
        }

        public override string ToString()
        {
            return "[VehicleAdd - Vehicle: " + Vehicle +
                "]\n\t" + base.ToString();
        }
    }
}

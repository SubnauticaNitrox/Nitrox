using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleCreated : Packet
    {
        public VehicleModel Vehicle { get; }

        public VehicleCreated(VehicleModel vehicle)
        {
            Vehicle = vehicle;
        }

        public override string ToString()
        {
            return "[VehicleCreated - Vehicle: " + Vehicle +  "]";
        }
    }
}

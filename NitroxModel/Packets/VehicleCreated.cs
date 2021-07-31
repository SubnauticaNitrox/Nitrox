using System;
using NitroxModel.DataStructures.GameLogic;

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
    }
}

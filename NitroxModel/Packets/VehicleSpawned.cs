using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleSpawned : Packet
    {
        public byte[] SerializeData { get; }
        public VehicleModel VehicleModel { get; }

        public VehicleSpawned(byte[] serializeData, VehicleModel vehicleModel)
        {
            SerializeData = serializeData;
            VehicleModel = vehicleModel;
        }

        public override string ToString()
        {
            return "[VehicleSpawned - VehicleId: " + VehicleModel.Id + " VehicleType: " + VehicleModel.TechType + " VehiclePosition: " + VehicleModel.Position;
        }
    }
}

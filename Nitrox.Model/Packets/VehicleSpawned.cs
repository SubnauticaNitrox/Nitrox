using System;
using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class VehicleSpawned : Packet
    {
        public byte[] SerializedData { get; }
        public VehicleModel VehicleModel { get; }

        public VehicleSpawned(byte[] serializedData, VehicleModel vehicleModel)
        {
            SerializedData = serializedData;
            VehicleModel = vehicleModel;
        }

        public override string ToString()
        {
            return $"[VehicleSpawned - {VehicleModel}";
        }
    }
}

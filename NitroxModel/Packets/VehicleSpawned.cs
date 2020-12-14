using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleSpawned : Packet
    {
        public VehicleModel VehicleModel { get; }
        public byte[] SerializedData { get; }

        public VehicleSpawned(byte[] serializedData, VehicleModel vehicleModel)
        {
            SerializedData = serializedData;
            VehicleModel = vehicleModel;
        }

        public override string ToString()
        {
            return $"[VehicleSpawned - VehicleModel: {VehicleModel}, SerializedData: {SerializedData?.Length}]";
        }
    }
}

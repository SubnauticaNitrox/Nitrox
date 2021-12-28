using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class VehicleSpawned : Packet
    {
        [Index(0)]
        public virtual byte[] SerializedData { get; protected set; } // TODO: change data type, similar to PingRenamed
        [Index(1)]
        public virtual VehicleModel VehicleModel { get; protected set; }

        public VehicleSpawned() { }

        public VehicleSpawned(byte[] serializedData, VehicleModel vehicleModel)
        {
            SerializedData = serializedData;
            VehicleModel = vehicleModel;
        }
    }
}

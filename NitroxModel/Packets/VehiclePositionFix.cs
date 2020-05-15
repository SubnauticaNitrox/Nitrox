using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehiclePositionFix : Packet
    {
        public NitroxId VehicleId { get; }
        public NitroxVector3 Position { get; }

        public VehiclePositionFix(NitroxId vehicleId, NitroxVector3 position)
        {
            VehicleId = vehicleId;
            Position = position;
        }

        public override string ToString()
        {
            return $"[VehiclePositionFix: VehicleId: {VehicleId}, Position: {Position}]";
        }
    }
}

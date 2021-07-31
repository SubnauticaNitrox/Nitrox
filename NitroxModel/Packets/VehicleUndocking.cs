using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleUndocking : Packet
    {
        public NitroxId VehicleId { get; }
        public NitroxId DockId { get; }
        public ushort PlayerId { get; }
        public bool UndockingStart { get; }

        public VehicleUndocking(NitroxId vehicleId, NitroxId dockId, ushort playerId, bool undockingStart)
        {
            VehicleId = vehicleId;
            DockId = dockId;
            PlayerId = playerId;
            UndockingStart = undockingStart;
        }
    }
}

using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets
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

        public override string ToString()
        {
            return $"[VehicleUndocking VehicleId: {VehicleId} DockId: {DockId} PlayerId: {PlayerId} Start: {UndockingStart}]";
        }
    }
}

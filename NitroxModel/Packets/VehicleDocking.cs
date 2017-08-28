using System;

namespace NitroxModel.Packets
{
    public enum DockingAction
    {
        Docking,

        // TODO: This naming is not descriptive at all...

        // Player clicked to dock the vehicle
        UndockingInitiate,

        // Player is in the vehicle (cinematics controller ended), now the "exit the bay" animation can start.
        UndockingPlayerIn
    }

    [Serializable]
    public class VehicleDocking : Packet
    {
        public string PlayerId { get; }
        public string SubRootGuid { get; }
        public string VehicleGuid { get; }
        public DockingAction DockingAction { get; }

        public VehicleDocking(string playerId, string subRootGuid, string vehicleGuid, DockingAction dockingAction)
        {
            PlayerId = playerId;
            SubRootGuid = subRootGuid;
            VehicleGuid = vehicleGuid;
            DockingAction = dockingAction;
        }

        public override string ToString()
        {
            return "[VehicleDocking Player: " + PlayerId + " SubRootGuid: " + SubRootGuid + " VehicleGuid: " + VehicleGuid + " DockingAction: " + DockingAction + "]";
        }
    }
}

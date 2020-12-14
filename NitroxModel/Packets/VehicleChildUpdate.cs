using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleChildUpdate : Packet
    {
        public NitroxId VehicleId { get; }
        public List<InteractiveChildObjectIdentifier> InteractiveChildIdentifiers { get; set; }

        public VehicleChildUpdate(NitroxId vehicleId, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers)
        {
            VehicleId = vehicleId;
            InteractiveChildIdentifiers = interactiveChildIdentifiers;
        }

        public override string ToString()
        {
            return $"[VehicleChildUpdate - VehicleId: {VehicleId}, InteractiveChildIdentifiers: {InteractiveChildIdentifiers?.Count}]";
        }

        public override string ToLongString()
        {
            return $"[VehicleChildUpdate - VehicleId: {VehicleId}, InteractiveChildIdentifiers: ({string.Join(", ", InteractiveChildIdentifiers)})]";
        }
    }
}

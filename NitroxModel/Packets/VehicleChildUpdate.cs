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
        public List<InteractiveChildObjectIdentifier> InteractiveChildIdentifiers { get; }

        public VehicleChildUpdate(NitroxId vehicleId, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers)
        {
            VehicleId = vehicleId;
            InteractiveChildIdentifiers = interactiveChildIdentifiers;
        }
    }
}

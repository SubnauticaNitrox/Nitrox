using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class VehicleChildUpdate : Packet
    {
        [Index(0)]
        public virtual NitroxId VehicleId { get; protected set; }
        [Index(1)]
        public virtual List<InteractiveChildObjectIdentifier> InteractiveChildIdentifiers { get; protected set; }
        
        private VehicleChildUpdate() { }

        public VehicleChildUpdate(NitroxId vehicleId, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers)
        {
            VehicleId = vehicleId;
            InteractiveChildIdentifiers = interactiveChildIdentifiers;
        }
    }
}

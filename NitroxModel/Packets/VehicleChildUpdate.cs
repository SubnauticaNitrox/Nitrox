using ProtoBufNet;
using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    [ProtoContract]
    public class VehicleChildUpdate : Packet
    {
        [ProtoMember(1)]
        public NitroxId VehicleId { get; }

        [ProtoMember(2)]
        public List<InteractiveChildObjectIdentifier> InteractiveChildIdentifiers { get; set; }

        public VehicleChildUpdate()
        {

        }

        public VehicleChildUpdate(NitroxId vehicleId,List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers)
        {
            VehicleId = vehicleId;
            InteractiveChildIdentifiers = interactiveChildIdentifiers;
        }
    }
}

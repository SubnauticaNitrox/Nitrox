using NitroxModel.DataStructures.Util;
using ProtoBuf;
using System;
using System.Collections.Generic;
using UnityEngine;
using NitroxModel.Packets;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    [ProtoContract]
    public class VehicleChildUpdate : Packet
    {
        [ProtoMember(1)]
        public string VehicleGuid { get; }

        [ProtoMember(2)]
        public List<InteractiveChildObjectIdentifier> SerializableInteractiveChildIdentifiers
        {
            get { return (InteractiveChildIdentifiers.IsPresent()) ? InteractiveChildIdentifiers.Get() : null; }
            set { InteractiveChildIdentifiers = Optional<List<InteractiveChildObjectIdentifier>>.OfNullable(value); }
        }
        [ProtoIgnore]
        public Optional<List<InteractiveChildObjectIdentifier>> InteractiveChildIdentifiers { get; set; }

        public VehicleChildUpdate()
        {
            InteractiveChildIdentifiers = Optional<List<InteractiveChildObjectIdentifier>>.Empty();
        }

        public VehicleChildUpdate(string vehicleGuid,Optional<List<InteractiveChildObjectIdentifier>> interactiveChildIdentifiers)
        {
            VehicleGuid = vehicleGuid;
            InteractiveChildIdentifiers = interactiveChildIdentifiers;
        }
    }
}

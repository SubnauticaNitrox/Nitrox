using NitroxModel.DataStructures.Util;
using ProtoBuf;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class VehicleModel
    {
        [ProtoMember(1)]
        public TechType TechType { get; }

        [ProtoMember(2)]
        public string Guid { get; set; }

        [ProtoMember(3)]
        public Vector3 Position { get; set; }

        [ProtoMember(4)]
        public Quaternion Rotation { get; set; }

        [ProtoMember(5)]
        public List<InteractiveChildObjectIdentifier> SerializableInteractiveChildIdentifiers
        {
            get { return (InteractiveChildIdentifiers.IsPresent()) ? InteractiveChildIdentifiers.Get() : null; }
            set { InteractiveChildIdentifiers = Optional<List<InteractiveChildObjectIdentifier>>.OfNullable(value); }
        }

        [ProtoMember(6)]
        public string SerializableDockingBayGuid
        {
            get { return (DockingBayGuid.IsPresent()) ? DockingBayGuid.Get() : null; }
            set { DockingBayGuid = Optional<string>.OfNullable(value); }
        }

        [ProtoIgnore]
        public Optional<List<InteractiveChildObjectIdentifier>> InteractiveChildIdentifiers { get; set; }

        [ProtoIgnore]
        public Optional<string> DockingBayGuid { get; set; }

        public VehicleModel()
        {
            InteractiveChildIdentifiers = Optional<List<InteractiveChildObjectIdentifier>>.Empty();
            DockingBayGuid = Optional<string>.Empty();
        }

        public VehicleModel(TechType techType, string guid, Vector3 position, Quaternion rotation, Optional<List<InteractiveChildObjectIdentifier>> interactiveChildIdentifiers, Optional<string> dockingBayGuid)
        {
            TechType = techType;
            Guid = guid;
            Position = position;
            Rotation = rotation;
            InteractiveChildIdentifiers = interactiveChildIdentifiers;
            DockingBayGuid = dockingBayGuid;
        }
    }
}

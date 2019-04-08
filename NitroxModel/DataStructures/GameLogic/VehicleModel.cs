﻿using NitroxModel.DataStructures.Util;
using ProtoBufNet;
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
        public NitroxId Id { get; set; }

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
        public NitroxId SerializableDockingBayId
        {
            get { return (DockingBayId.IsPresent()) ? DockingBayId.Get() : null; }
            set { DockingBayId = Optional<NitroxId>.OfNullable(value); }
        }

        [ProtoMember(7)]
        public string Name { get; set; }

        [ProtoMember(8)]
        public Vector3[] HSB { get; set; }

        [ProtoMember(9)]
        public Vector3[] Colours { get; set; }

        [ProtoIgnore]
        public Optional<List<InteractiveChildObjectIdentifier>> InteractiveChildIdentifiers { get; set; }

        [ProtoIgnore]
        public Optional<NitroxId> DockingBayId { get; set; }

        public VehicleModel()
        {
            InteractiveChildIdentifiers = Optional<List<InteractiveChildObjectIdentifier>>.Empty();
            DockingBayId = Optional<NitroxId>.Empty();
        }

        public VehicleModel(TechType techType, NitroxId id, Vector3 position, Quaternion rotation, Optional<List<InteractiveChildObjectIdentifier>> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, Vector3[] hsb, Vector3[] colours)
        {
            TechType = techType;
            Id = id;
            Position = position;
            Rotation = rotation;
            InteractiveChildIdentifiers = interactiveChildIdentifiers;
            DockingBayId = dockingBayId;
            Name = name;
            HSB = hsb;
            Colours = colours;
        }
    }
}

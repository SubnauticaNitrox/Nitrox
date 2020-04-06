using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;
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
        public ThreadSafeCollection<InteractiveChildObjectIdentifier> InteractiveChildIdentifiers { get; }

        [ProtoMember(6)]
        public Optional<NitroxId> DockingBayId { get; set; }

        [ProtoMember(7)]
        public string Name { get; set; }

        [ProtoMember(8)]
        public Vector3[] HSB { get; set; }

        [ProtoMember(9)]
        public Vector3[] Colours { get; set; }

        [ProtoMember(10)]
        public float Health { get; set; } = 1;

        public VehicleModel()
        {
            InteractiveChildIdentifiers = new ThreadSafeCollection<InteractiveChildObjectIdentifier>();
            DockingBayId = Optional.Empty;
        }

        public VehicleModel(TechType techType, NitroxId id, Vector3 position, Quaternion rotation, IEnumerable<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, Vector3[] hsb,
            Vector3[] colours,
            float health)
        {
            TechType = techType;
            Id = id;
            Position = position;
            Rotation = rotation;
            InteractiveChildIdentifiers = new ThreadSafeCollection<InteractiveChildObjectIdentifier>(interactiveChildIdentifiers);
            DockingBayId = dockingBayId;
            Name = name;
            HSB = hsb;
            Colours = colours;
            Health = health;
        }
    }
}

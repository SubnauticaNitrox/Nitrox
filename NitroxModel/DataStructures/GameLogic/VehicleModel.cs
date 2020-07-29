using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class VehicleModel : NitroxBehavior
    {
        [ProtoMember(1)]
        public NitroxTechType TechType { get; }

        [ProtoMember(5)]
        public ThreadSafeCollection<InteractiveChildObjectIdentifier> InteractiveChildIdentifiers { get; }

        [ProtoMember(6)]
        public Optional<NitroxId> DockingBayId { get; set; }

        [ProtoMember(7)]
        public string Name { get; set; }

        [ProtoMember(8)]
        public NitroxVector3[] HSB { get; set; }

        [ProtoMember(9)]
        public float Health { get; set; }

        protected VehicleModel()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
            InteractiveChildIdentifiers = new ThreadSafeCollection<InteractiveChildObjectIdentifier>();
            DockingBayId = Optional.Empty;
            Health = 200;
        }

        public VehicleModel(NitroxTechType techType, IEnumerable<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, NitroxVector3[] hsb, float health)
        {
            TechType = techType;
            InteractiveChildIdentifiers = new ThreadSafeCollection<InteractiveChildObjectIdentifier>(interactiveChildIdentifiers);
            DockingBayId = dockingBayId;
            Name = name;
            HSB = hsb;
            Health = health;
        }

        public override string ToString()
        {
            return $"[VehicleModel - TechType: {TechType}, Id: {Id}, Position: {Position}, Rotation: {Rotation}, Name: {Name}, Health: {Health}, DockingBayId: {DockingBayId}]";
        }
    }
}

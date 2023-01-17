using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [DataContract]
    public abstract class VehicleModel
    {
        [DataMember(Order = 1)]
        public NitroxTechType TechType { get; set; }

        [DataMember(Order = 2)]
        public NitroxId Id { get; set; }

        [DataMember(Order = 3)]
        public NitroxVector3 Position { get; set; }

        [DataMember(Order = 4)]
        public NitroxQuaternion Rotation { get; set; }

        [DataMember(Order = 5)]
        public ThreadSafeList<InteractiveChildObjectIdentifier> InteractiveChildIdentifiers { get; }

        [DataMember(Order = 6)]
        public Optional<NitroxId> DockingBayId { get; set; }

        [DataMember(Order = 7)]
        public string Name { get; set; }

        [DataMember(Order = 8)]
        public NitroxVector3[] HSB { get; set; }

        [DataMember(Order = 9)]
        public float Health { get; set; }

        [IgnoreConstructor]
        protected VehicleModel()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
            InteractiveChildIdentifiers = new ThreadSafeList<InteractiveChildObjectIdentifier>();
            DockingBayId = Optional.Empty;
            Health = 200;
        }

        public VehicleModel(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, IEnumerable<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, NitroxVector3[] hsb, float health)
        {
            TechType = techType;
            Id = id;
            Position = position;
            Rotation = rotation;
            InteractiveChildIdentifiers = new ThreadSafeList<InteractiveChildObjectIdentifier>(interactiveChildIdentifiers);
            DockingBayId = dockingBayId;
            Name = name;
            HSB = hsb;
            Health = health;
        }

        /// <remarks>Used for deserialization</remarks>
        public VehicleModel(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, ThreadSafeList<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, NitroxVector3[] hsb, float health)
        {
            TechType = techType;
            Id = id;
            Position = position;
            Rotation = rotation;
            InteractiveChildIdentifiers = interactiveChildIdentifiers;
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

using System.Collections.Generic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic
{
    [ZeroFormattable]
    [ProtoContract]
    public class VehicleModel
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual NitroxTechType TechType { get; protected set; }

        [Index(1)]
        [ProtoMember(2)]
        public virtual NitroxId Id { get; set; }

        [Index(2)]
        [ProtoMember(3)]
        public virtual NitroxVector3 Position { get; set; }

        [Index(3)]
        [ProtoMember(4)]
        public virtual NitroxQuaternion Rotation { get; set; }

        [Index(4)]
        [ProtoMember(5)]
        public virtual ThreadSafeList<InteractiveChildObjectIdentifier> InteractiveChildIdentifiers { get; protected set; }

        [Index(5)]
        [ProtoMember(6)]
        public virtual Optional<NitroxId> DockingBayId { get; set; }

        [Index(6)]
        [ProtoMember(7)]
        public virtual string Name { get; set; }

        [Index(7)]
        [ProtoMember(8)]
        public virtual NitroxVector3[] HSB { get; set; }

        [Index(8)]
        [ProtoMember(9)]
        public virtual float Health { get; set; }

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

        public override string ToString()
        {
            return $"[VehicleModel - TechType: {TechType}, Id: {Id}, Position: {Position}, Rotation: {Rotation}, Name: {Name}, Health: {Health}, DockingBayId: {DockingBayId}]";
        }
    }
}

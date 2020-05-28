using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures;
using System.Text;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructorBeginCrafting : Packet
    {
        public NitroxId ConstructorId { get; }
        public NitroxId ConstructedItemId { get; }
        public NitroxTechType TechType { get; }
        public float Duration { get; }
        public List<InteractiveChildObjectIdentifier> InteractiveChildIdentifiers { get; }
        public NitroxVector3 Position { get; }
        public NitroxQuaternion Rotation { get; }
        public string Name { get; }
        public NitroxVector3[] HSB { get; }
        public float Health { get; }

        public ConstructorBeginCrafting(NitroxId constructorId, NitroxId constructeditemId, NitroxTechType techType, float duration, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, NitroxVector3 position, NitroxQuaternion rotation, 
            string name, NitroxVector3[] hsb, float health)
        {
            ConstructorId = constructorId;
            ConstructedItemId = constructeditemId;
            TechType = techType;
            Duration = duration;
            InteractiveChildIdentifiers = interactiveChildIdentifiers;
            Position = position;
            Rotation = rotation;
            Name = name;
            HSB = hsb;
            Health = health;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder($"[ConstructorBeginCrafting - ConstructorId: {ConstructorId} ConstructedItemId: {ConstructedItemId} TechType: {TechType} Duration: {Duration} Health: {Health} InteractiveChildIdentifiers: (");

            foreach (InteractiveChildObjectIdentifier childIdentifier in InteractiveChildIdentifiers)
            {
                s.Append($"{childIdentifier} ");
            }

            s.Append($") Position{Position} Rotation{Rotation}");

            return s.ToString();
        }
    }
}

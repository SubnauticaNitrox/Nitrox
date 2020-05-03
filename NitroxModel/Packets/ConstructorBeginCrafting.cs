using System;
using System.Collections.Generic;
using DTO = NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructorBeginCrafting : Packet
    {
        public ConstructorBeginCrafting(DTO.NitroxId constructorId, DTO.NitroxId constructeditemId, DTO.TechType techType, float duration, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, DTO.Vector3 position, DTO.Quaternion rotation,
                                        string name, DTO.Vector3[] hsb, DTO.Vector3[] colours, float health)
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
            Colours = colours;
            Health = health;
        }

        public DTO.NitroxId ConstructorId { get; }
        public DTO.NitroxId ConstructedItemId { get; }
        public DTO.TechType TechType { get; }
        public float Duration { get; }
        public List<InteractiveChildObjectIdentifier> InteractiveChildIdentifiers { get; }
        public DTO.Vector3 Position { get; }
        public DTO.Quaternion Rotation { get; }
        public string Name { get; }
        public DTO.Vector3[] HSB { get; }
        public DTO.Vector3[] Colours { get; }
        public float Health { get; }

        public override string ToString()
        {
            string s = "[ConstructorBeginCrafting - ConstructorId: " + ConstructorId + " ConstructedItemId: " + ConstructedItemId + " TechType: " + TechType + " Duration: " + Duration + " Health: " + Health + " InteractiveChildIdentifiers: (";

            foreach (InteractiveChildObjectIdentifier childIdentifier in InteractiveChildIdentifiers)
            {
                s += childIdentifier + " ";
            }

            return s + ")" + " Position" + Position + " Rotation" + Rotation;
        }
    }
}

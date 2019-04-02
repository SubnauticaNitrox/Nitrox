using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructorBeginCrafting : Packet
    {
        public string ConstructorGuid { get; }
        public string ConstructedItemGuid { get; }
        public TechType TechType { get; }
        public float Duration { get; }
        public List<InteractiveChildObjectIdentifier> InteractiveChildIdentifiers { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public string Name { get; }
        public Vector3[] HSB { get; }
        public Vector3[] Colours { get; }

        public ConstructorBeginCrafting(string constructorGuid, string constructedItemGuid, TechType techType, float duration, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Vector3 position, Quaternion rotation, 
            string name, Vector3[] hsb, Vector3[] colours)
        {
            ConstructorGuid = constructorGuid;
            ConstructedItemGuid = constructedItemGuid;
            TechType = techType;
            Duration = duration;
            InteractiveChildIdentifiers = interactiveChildIdentifiers;
            Position = position;
            Rotation = rotation;
            Name = name;
            HSB = hsb;
            Colours = colours;
        }

        public override string ToString()
        {
            string s = "[ConstructorBeginCrafting - ConstructorGuid: " + ConstructorGuid + " ConstructedItemGuid: " + ConstructedItemGuid + " TechType: " + TechType + " Duration: " + Duration + " InteractiveChildIdentifiers: (";

            foreach (InteractiveChildObjectIdentifier childIdentifier in InteractiveChildIdentifiers)
            {
                s += childIdentifier + " ";
            }

            return s + ")" + " Position" + Position + " Rotation" + Rotation;
        }
    }
}

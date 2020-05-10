using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures;
using UnityEngine;
using System.Text;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructorBeginCrafting : Packet
    {
        public NitroxId ConstructorId { get; }
        public NitroxId ConstructedItemId { get; }
        public TechType TechType { get; }
        public float Duration { get; }
        public List<InteractiveChildObjectIdentifier> InteractiveChildIdentifiers { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public string Name { get; }
        public Vector3[] HSB { get; }
        public float Health { get; }

        public ConstructorBeginCrafting(NitroxId constructorId, NitroxId constructeditemId, TechType techType, float duration, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Vector3 position, Quaternion rotation, 
            string name, Vector3[] hsb, float health)
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

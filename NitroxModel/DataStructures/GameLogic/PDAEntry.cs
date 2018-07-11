using System;
using UnityEngine;
using System.Collections.Generic;
using ProtoBuf;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class PDAEntry
    {
        [ProtoMember(1)]
        public TechType TechType;
        [ProtoMember(2)]
        public float Progress;
        [ProtoMember(3)]
        public int Unlocked;
        public List<Entity> ChildEntities { get; set; } = new List<Entity>();
        public PDAEntry()
        {
            // Default Constructor for serialization
        }
    }
}

﻿using System;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    public class Entity
    {
        public AbsoluteEntityCell AbsoluteEntityCell => new AbsoluteEntityCell(Position, Level);

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public TechType TechType { get; }
        public string Guid { get; }
        public int Level { get; }
        public string ClassId { get; }
        public Optional<Entity> ChildEntity { get; set; }

        public Entity(Vector3 position, Quaternion rotation, TechType techType, int level, string classId)
        {
            Position = position;
            Rotation = rotation;
            TechType = techType;
            Guid = System.Guid.NewGuid().ToString();
            Level = level;
            ClassId = classId;
            ChildEntity = Optional<Entity>.Empty();
        }

        public override string ToString()
        {
            return "[Entity Position: " + Position + " TechType: " + TechType + " Guid: " + Guid + " Level: " + Level + " classId: " + ClassId + " ChildEntityGuid: " + ChildEntity + "]";
        }
    }
}

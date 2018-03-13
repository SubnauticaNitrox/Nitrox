using System;
using UnityEngine;
using System.Collections.Generic;

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
        public List<Entity> ChildEntities { get; set; } = new List<Entity>();

        public Entity(Vector3 position, Quaternion rotation, TechType techType, int level, string classId)
        {
            Position = position;
            Rotation = rotation;
            TechType = techType;
            Guid = System.Guid.NewGuid().ToString();
            Level = level;
            ClassId = classId;
        }

        public override string ToString()
        {
            return "[Entity Position: " + Position + " TechType: " + TechType + " Guid: " + Guid + " Level: " + Level + " classId: " + ClassId + " ChildEntities: " + ChildEntities + "]";
        }
    }
}

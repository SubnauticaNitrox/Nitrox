using System;
using UnityEngine;

namespace NitroxModel.GameLogic
{
    [Serializable]
    public class Entity
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public TechType TechType { get; }
        public string Guid { get; }
        public int Level { get; }
        public Entity(Vector3 position, Quaternion rotation, TechType techType, string guid, int level)
        {
            Position = position;
            Rotation = rotation;
            TechType = techType;
            Guid = guid;
            Level = level;
        }

        public override string ToString()
        {
            return "[Entity Position: " + Position + " TechType: " + TechType + " Guid: " + Guid + " Level: " + Level + "]";
        }
    }
}

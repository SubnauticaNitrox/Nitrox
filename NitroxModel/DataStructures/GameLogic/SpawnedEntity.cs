using UnityEngine;
using System;

namespace NitroxModel.GameLogic
{
    [Serializable]
    public class SpawnedEntity
    {
        public Vector3 Position { get; }
        public TechType TechType { get; }
        public String Guid { get; }

        public SpawnedEntity(Vector3 position, TechType techType, String guid)
        {
            this.Position = position;
            this.TechType = techType;
            this.Guid = guid;
        }

        public override string ToString()
        {
            return "[SpawnedEntity Position: " + Position + " TechType: " + TechType + " Guid: " + Guid + " ]";
        }
    }
}

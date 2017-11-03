using UnityEngine;
using System;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.GameLogic
{
    [Serializable]
    public class Entity
    {
        public Vector3 Position { get; set; }
        public TechType TechType { get; }
        public String Guid { get; }
        public int Level { get; }
        public Optional<String> SimulatingPlayerId { get; set; }

        public Entity(Vector3 position, TechType techType, String guid, int level, Optional<String> simulatingPlayerId)
        {
            this.Position = position;
            this.TechType = techType;
            this.Guid = guid;
            this.Level = level;
            this.SimulatingPlayerId = simulatingPlayerId;
        }

        public override string ToString()
        {
            return "[Entity Position: " + Position + " TechType: " + TechType + " Guid: " + Guid + " Level: " + Level + " SimulatingPlayerId: " + SimulatingPlayerId + " ]";
        }
    }
}

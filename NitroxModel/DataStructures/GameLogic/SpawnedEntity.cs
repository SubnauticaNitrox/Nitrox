using UnityEngine;
using System;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.GameLogic
{
    [Serializable]
    public class SpawnedEntity
    {
        public Vector3 Position { get; }
        public TechType TechType { get; }
        public String Guid { get; }
        public int Level { get; }
        public Optional<String> SimulatingPlayerId { get; set; }

        public SpawnedEntity(Vector3 position, TechType techType, String guid, int level, Optional<String> controllingPlayerId)
        {
            this.Position = position;
            this.TechType = techType;
            this.Guid = guid;
            this.Level = level;
            this.SimulatingPlayerId = controllingPlayerId;
        }

        public override string ToString()
        {
            return "[SpawnedEntity Position: " + Position + " TechType: " + TechType + " Guid: " + Guid + " Level: " + Level + " SimulatingPlayerId: " + SimulatingPlayerId + " ]";
        }
    }
}

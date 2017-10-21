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
        public Optional<String> ControllingPlayerId { get; }

        public SpawnedEntity(Vector3 position, TechType techType, String guid, Optional<String> controllingPlayerId)
        {
            this.Position = position;
            this.TechType = techType;
            this.Guid = guid;
            this.ControllingPlayerId = controllingPlayerId;
        }

        public override string ToString()
        {
            return "[SpawnedEntity Position: " + Position + " TechType: " + TechType + " Guid: " + Guid + " ControllingPlayerId: " + ControllingPlayerId + " ]";
        }
    }
}

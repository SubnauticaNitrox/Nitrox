using System;
using System.Collections.Generic;
using System.Text;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleMovement : Movement
    {
        public String TechType { get; protected set; }
        public String Guid { get; protected set; }

        public VehicleMovement(String playerId, Vector3 playerPosition, Quaternion rotation, String techType, String guid) : base(playerId, playerPosition, rotation)
        {
            this.Guid = guid;
            this.TechType = techType;
            this.PlayerMustBeInRangeToReceive = false;
        }

        public override string ToString()
        {
            return "[VehicleMovement - playerId: " + PlayerId + " position: " + PlayerPosition + " Rotation: " + Rotation + " TechType: " + TechType + " guid: " + Guid + "]";
        }
    }
}

using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleMovement : Movement
    {
        public String TechType { get; protected set; }
        public String Guid { get; protected set; }
        public Vector3 Velocity { get; protected set; }
        public Vector3 AngularVelocity { get; protected set; }

        public VehicleMovement(String playerId, Vector3 playerPosition, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, String techType, String guid) : base(playerId, playerPosition, rotation, rotation, Optional<String>.Empty())
        {
            this.Guid = guid;
            this.TechType = techType;
            this.Velocity = velocity;
            this.AngularVelocity = angularVelocity;
            this.PlayerMustBeInRangeToReceive = false;
        }

        public override string ToString()
        {
            return "[VehicleMovement - TechType: " + TechType + " Velocity: " + Velocity + " AngularVelocity: " + AngularVelocity + " guid: " + Guid + "]\n\t" + base.ToString();
        }
    }
}

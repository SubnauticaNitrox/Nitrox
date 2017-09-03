using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleMovement : Movement
    {
        public String TechType { get; }
        public String Guid { get; }
        public float SteeringWheelYaw { get; }
        public float SteeringWheelPitch { get; }

        public VehicleMovement(String playerId, Vector3 playerPosition, Vector3 velocity, Quaternion rotation, String techType, String guid, float steeringWheelYaw, float steeringWheelPitch) : base(playerId, playerPosition, velocity, rotation, rotation, Optional<String>.Empty())
        {
            this.Guid = guid;
            this.SteeringWheelYaw = steeringWheelYaw;
            this.SteeringWheelPitch = steeringWheelPitch;
            this.TechType = techType;
            this.PlayerMustBeInRangeToReceive = false;
        }

        public override string ToString()
        {
            return "[VehicleMovement - TechType: " + TechType + " guid: " + Guid + "]\n\t" + base.ToString();
        }
    }
}

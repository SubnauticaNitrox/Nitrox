using System;

namespace NitroxModel.DataStructures.ServerModel
{
    public class VehicleModel
    {
        public String TechType { get; }
        public String Guid { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public Vector3 Velocity { get; }
        public float SteeringWheelYaw { get; }
        public float SteeringWheelPitch { get; }

        public VehicleModel(String techType, String guid, Vector3 position, Quaternion rotation, Vector3 velocity, float steeringWheelYaw, float steeringWheelPitch)
        {
            this.TechType = techType;
            this.Guid = guid;
            this.Position = position;
            this.Rotation = rotation;
            this.Velocity = velocity;
            this.SteeringWheelYaw = steeringWheelYaw;
            this.SteeringWheelPitch = steeringWheelPitch;
        }
    }
}

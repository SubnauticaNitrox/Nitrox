using System;

namespace NitroxModel.DataStructures.ServerModel
{
    public class VehicleModel
    {
        public String TechType { get; set; }
        public String Guid { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 AngularVelocity { get; set; }

        public VehicleModel(String techType, String guid, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
        {
            this.TechType = techType;
            this.Guid = guid;
            this.Position = position;
            this.Rotation = rotation;
            this.Velocity = velocity;
            this.AngularVelocity = angularVelocity;
        }
    }
}

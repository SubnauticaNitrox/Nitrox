using System;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleMovement : Movement
    {
        public TechType TechType { get; }
        public Vector3 AngularVelocity { get; }
        public string Guid { get; }
        public float SteeringWheelYaw { get; }
        public float SteeringWheelPitch { get; }
        public bool AppliedThrottle { get; }

        public VehicleMovement(string playerId, Vector3 playerPosition, Vector3 velocity, Quaternion rotation, Vector3 angularVelocity, TechType techType, string guid, float steeringWheelYaw, float steeringWheelPitch, bool appliedThrottle) : base(playerId, playerPosition, velocity, rotation, rotation, Optional<string>.Empty())
        {
            TechType = techType;
            AngularVelocity = angularVelocity;
            Guid = guid;

            SteeringWheelYaw = steeringWheelYaw;
            SteeringWheelPitch = steeringWheelPitch;
            AppliedThrottle = appliedThrottle;
        }

        public override string ToString()
        {
            return "[VehicleMovement - TechType: " + TechType +
                " AngularVelocity: " + AngularVelocity +
                " Guid: " + Guid +
                " SteeringWheelYaw: " + SteeringWheelYaw +
                " SteeringWheelPitch: " + SteeringWheelPitch +
                " AppliedThrottle: " + AppliedThrottle +
                "]\n\t" + base.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.DataStructures.GameLogic;
using UnityEngine;

namespace NitroxModel_Subnautica.Helper
{
    public class VehicleMovementFactory
    {
        public static VehicleMovementData GetVehicleMovementData(TechType techType, string guid, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, float steeringWheelYaw, float steeringWheelPitch, bool appliedThrottle,
            Vector3 leftAimTarget, Vector3 rightAimTarget)
        {
            switch (techType)
            {
                case TechType.Exosuit:
                    return new ExosuitMovementData(techType.Model(), guid, position, rotation, velocity, angularVelocity, steeringWheelYaw, steeringWheelPitch, appliedThrottle, leftAimTarget, rightAimTarget);
                default:
                    return new VehicleMovementData(techType.Model(), guid, position, rotation, velocity, angularVelocity, steeringWheelYaw, steeringWheelPitch, appliedThrottle);
            }
        }
    }
}

﻿using NitroxModel.DataStructures.GameLogic;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using UnityEngine;
using NitroxModel.DataStructures;

namespace NitroxModel_Subnautica.Helper
{
    public class VehicleMovementFactory
    {
        public static VehicleMovementData GetVehicleMovementData(TechType techType, NitroxId id, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, float steeringWheelYaw, float steeringWheelPitch, bool appliedThrottle,
            Vector3 leftAimTarget, Vector3 rightAimTarget)
        {
            switch (techType)
            {
                case TechType.Exosuit:
                    return new ExosuitMovementData(techType.Model(), id, position, rotation, velocity, angularVelocity, steeringWheelYaw, steeringWheelPitch, appliedThrottle, leftAimTarget, rightAimTarget);
                default:
                    return new VehicleMovementData(techType.Model(), id, position, rotation, velocity, angularVelocity, steeringWheelYaw, steeringWheelPitch, appliedThrottle);
            }
        }
    }
}

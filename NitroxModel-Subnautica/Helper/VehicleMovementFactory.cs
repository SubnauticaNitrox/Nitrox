using NitroxModel.DataStructures.GameLogic;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using UnityEngine;
using NitroxModel.DataStructures;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxModel_Subnautica.Helper
{
    public static class VehicleMovementFactory
    {
        public static VehicleMovementData GetVehicleMovementData(TechType techType, NitroxId id, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, float steeringWheelYaw, float steeringWheelPitch, bool appliedThrottle, Vector3 leftAimTarget, Vector3 rightAimTarget, float health)
        {
            switch (techType)
            {
                case TechType.Exosuit:
                    return new ExosuitMovementData(techType.ToDto(), id, position.ToDto(), rotation.ToDto(), velocity.ToDto(), angularVelocity.ToDto(), steeringWheelYaw, steeringWheelPitch, appliedThrottle, leftAimTarget.ToDto(), rightAimTarget.ToDto(), health);
                default:
                    return new VehicleMovementData(techType.ToDto(), id, position.ToDto(), rotation.ToDto(), velocity.ToDto(), angularVelocity.ToDto(), steeringWheelYaw, steeringWheelPitch, appliedThrottle, health);
            }
        }
    }
}

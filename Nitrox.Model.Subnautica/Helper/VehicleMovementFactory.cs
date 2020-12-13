using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using UnityEngine;

namespace Nitrox.Model.Subnautica.Helper
{
    public static class VehicleMovementFactory
    {
        public static VehicleMovementData GetVehicleMovementData(TechType techType, NitroxId id, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, float steeringWheelYaw, float steeringWheelPitch, bool appliedThrottle, Vector3 leftAimTarget, Vector3 rightAimTarget)
        {
            switch (techType)
            {
                case TechType.Exosuit:
                    return new ExosuitMovementData(techType.ToDto(), id, position.ToDto(), rotation.ToDto(), velocity.ToDto(), angularVelocity.ToDto(), steeringWheelYaw, steeringWheelPitch, appliedThrottle, leftAimTarget.ToDto(), rightAimTarget.ToDto());
                default:
                    return new VehicleMovementData(techType.ToDto(), id, position.ToDto(), rotation.ToDto(), velocity.ToDto(), angularVelocity.ToDto(), steeringWheelYaw, steeringWheelPitch, appliedThrottle);
            }
        }
    }
}

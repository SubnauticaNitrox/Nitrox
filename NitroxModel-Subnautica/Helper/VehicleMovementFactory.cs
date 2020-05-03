using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel_Subnautica.DataStructures.GameLogic;

namespace NitroxModel_Subnautica.Helper
{
    public class VehicleMovementFactory
    {
        public static VehicleMovementData GetVehicleMovementData(TechType techType, NitroxId id, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation, UnityEngine.Vector3 velocity, UnityEngine.Vector3 angularVelocity, float steeringWheelYaw, float steeringWheelPitch, bool appliedThrottle,
                                                                 UnityEngine.Vector3 leftAimTarget, UnityEngine.Vector3 rightAimTarget, float health)
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

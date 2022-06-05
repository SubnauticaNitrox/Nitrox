using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Serialization;

namespace NitroxModel_Subnautica.DataStructures.GameLogic;

[Serializable]
[JsonContractTransition]
public class ExosuitMovementData : VehicleMovementData
{
    [JsonMemberTransition]
    public NitroxVector3 LeftAimTarget { get; }

    [JsonMemberTransition]
    public NitroxVector3 RightAimTarget { get; }

    public ExosuitMovementData(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, NitroxVector3 velocity, NitroxVector3 angularVelocity, float steeringWheelYaw, float steeringWheelPitch, bool appliedThrottle, NitroxVector3 leftAimTarget, NitroxVector3 rightAimTarget)
        : base(techType, id, position, rotation, velocity, angularVelocity, steeringWheelYaw, steeringWheelPitch, appliedThrottle)
    {
        LeftAimTarget = leftAimTarget;
        RightAimTarget = rightAimTarget;
    }

    public override string ToString()
    {
        return $"[ExosuitMovementData - {base.ToString()}, LeftAimTarget: {LeftAimTarget}, RightAimTarget: {RightAimTarget}]";
    }
}

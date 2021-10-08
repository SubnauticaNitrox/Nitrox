using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using ProtoBufNet;

namespace NitroxModel_Subnautica.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class ExosuitMovementData : VehicleMovementData
    {
        [ProtoMember(1)]
        public NitroxVector3 LeftAimTarget { get; }

        [ProtoMember(2)]
        public NitroxVector3 RightAimTarget { get; }

        protected ExosuitMovementData()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

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
}

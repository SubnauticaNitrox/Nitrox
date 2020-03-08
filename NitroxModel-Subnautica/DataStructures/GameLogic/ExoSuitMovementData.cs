using System;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using UnityEngine;
using NitroxModel.DataStructures;

namespace NitroxModel_Subnautica.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class ExosuitMovementData : VehicleMovementData
    {
        [ProtoMember(1)]
        public Vector3 LeftAimTarget { get; }

        [ProtoMember(2)]
        public Vector3 RightAimTarget { get; }

        public ExosuitMovementData()
        {
            // For serialization purposes
        }

        public ExosuitMovementData(NitroxModel.DataStructures.TechType techType, NitroxId id, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, float steeringWheelYaw, float steeringWheelPitch, bool appliedThrottle,
            Vector3 leftAimTarget, Vector3 rightAimTarget, float health) : base(techType,id,position,rotation,velocity,angularVelocity,steeringWheelYaw,steeringWheelPitch,appliedThrottle,health)
        {
            LeftAimTarget = leftAimTarget;
            RightAimTarget = rightAimTarget;
        }
    }
}

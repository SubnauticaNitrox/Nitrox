using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using UnityEngine;

namespace NitroxModel_Subnautica.DataStructures.GameLogic
{
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

        public ExosuitMovementData(NitroxModel.DataStructures.TechType techType, string guid, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, float steeringWheelYaw, float steeringWheelPitch, bool appliedThrottle,
            Vector3 leftAimTarget, Vector3 rightAimTarget) : base(techType,guid,position,rotation,velocity,angularVelocity,steeringWheelYaw,steeringWheelPitch,appliedThrottle)
        {
            LeftAimTarget = leftAimTarget;
            RightAimTarget = rightAimTarget;
        }
    }
}

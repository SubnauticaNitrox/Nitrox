﻿using System;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using NitroxModel.DataStructures;

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

        public ExosuitMovementData(NitroxModel.DataStructures.GameLogic.NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, NitroxVector3 velocity, NitroxVector3 angularVelocity, float steeringWheelYaw, float steeringWheelPitch, bool appliedThrottle,
                                   NitroxVector3 leftAimTarget, NitroxVector3 rightAimTarget, float health) : base(techType,id,position,rotation,velocity,angularVelocity,steeringWheelYaw,steeringWheelPitch,appliedThrottle,health)
        {
            LeftAimTarget = leftAimTarget;
            RightAimTarget = rightAimTarget;
        }
    }
}

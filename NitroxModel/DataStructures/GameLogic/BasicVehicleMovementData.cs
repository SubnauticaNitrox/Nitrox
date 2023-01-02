﻿using System;
using System.Runtime.Serialization;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable]
[DataContract]
public class BasicVehicleMovementData : VehicleMovementData
{
    public BasicVehicleMovementData(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation) : 
        base(techType, id, position, rotation) { }

    public BasicVehicleMovementData(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, NitroxVector3 velocity, NitroxVector3 angularVelocity, float steeringWheelYaw, float steeringWheelPitch, bool appliedThrottle, NitroxVector3? driverPosition = null) : 
        base(techType, id, position, rotation, velocity, angularVelocity, steeringWheelYaw, steeringWheelPitch, appliedThrottle, driverPosition) { }
}

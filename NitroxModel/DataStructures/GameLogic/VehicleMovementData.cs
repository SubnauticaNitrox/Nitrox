using System;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable]
[JsonContractTransition]
public class VehicleMovementData
{
    [JsonMemberTransition]
    public NitroxTechType TechType { get; }
    [JsonMemberTransition]
    public NitroxId Id { get; set; }
    [JsonMemberTransition]
    public NitroxVector3 Position { get; }
    [JsonMemberTransition]
    public NitroxQuaternion Rotation { get; }
    [JsonMemberTransition]
    public NitroxVector3 Velocity { get; }
    [JsonMemberTransition]
    public NitroxVector3 AngularVelocity { get; }
    [JsonMemberTransition]
    public float SteeringWheelYaw { get; }
    [JsonMemberTransition]
    public float SteeringWheelPitch { get; }
    [JsonMemberTransition]
    public bool AppliedThrottle { get; }
    [JsonMemberTransition]
    public NitroxVector3? DriverPosition { get; set; }

    public VehicleMovementData(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, NitroxVector3 velocity, NitroxVector3 angularVelocity, float steeringWheelYaw, float steeringWheelPitch, bool appliedThrottle)
    {
        TechType = techType;
        Id = id;
        Position = position;
        Rotation = rotation;
        Velocity = velocity;
        AngularVelocity = angularVelocity;
        SteeringWheelYaw = steeringWheelYaw;
        SteeringWheelPitch = steeringWheelPitch;
        AppliedThrottle = appliedThrottle;
    }

    public VehicleMovementData(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation)
    {
        TechType = techType;
        Id = id;
        Position = position;
        Rotation = rotation;
        Velocity = NitroxVector3.Zero;
        AngularVelocity = NitroxVector3.Zero;
        SteeringWheelYaw = 0f;
        SteeringWheelPitch = 0f;
        AppliedThrottle = false;
    }

    public override string ToString()
    {
        return $"[VehicleMovementData - TechType: {TechType}, Id: {Id}, Position: {Position}, Rotation: {Rotation}, Velocity: {Velocity}, AngularVelocity: {AngularVelocity}, SteeringWheelYaw: {SteeringWheelYaw}, SteeringWheelPitch: {SteeringWheelPitch}, AppliedThrottle: {AppliedThrottle}]";
    }
}

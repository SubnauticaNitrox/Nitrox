using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Networking;

namespace NitroxModel.Packets;

[Serializable]
public class VehicleMovements : Packet
{
    public List<MovementData> Data { get; }
    public double RealTime { get; set; }
    
    public VehicleMovements(List<MovementData> data, double realTime)
    {
        Data = data;
        RealTime = realTime;
        DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.UNRELIABLE_SEQUENCED;
        UdpChannel = UdpChannelId.MOVEMENTS;
    }
}

[Serializable]
public abstract record class MovementData(NitroxId Id, NitroxVector3 Position, NitroxQuaternion Rotation) { }

[Serializable]
public record class SimpleMovementData(NitroxId Id, NitroxVector3 Position, NitroxQuaternion Rotation) : MovementData(Id, Position, Rotation) { }

[Serializable]
public record class DrivenVehicleMovementData(NitroxId Id, NitroxVector3 Position, NitroxQuaternion Rotation, sbyte SteeringWheelYaw, sbyte SteeringWheelPitch, bool ThrottleApplied) : MovementData(Id, Position, Rotation) { }

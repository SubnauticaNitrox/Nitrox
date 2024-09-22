using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Packets;

[Serializable]
public class VehicleMovements : Packet
{
    // TODO: change dictionary to list because it's too heavy right now
    public Dictionary<NitroxId, MovementData> Data { get; }
    public double RealTime { get; set; }
    
    public VehicleMovements(Dictionary<NitroxId, MovementData> data, double realTime)
    {
        Data = data;
        RealTime = realTime;
    }
}

public record struct MovementData(NitroxVector3 Position, NitroxVector3 Velocity, NitroxQuaternion Rotation, NitroxVector3 AngularVelocity);

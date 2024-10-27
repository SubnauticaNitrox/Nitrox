using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;

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
    }
}

public record struct MovementData(NitroxId Id, NitroxVector3 Position, NitroxQuaternion Rotation);

using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.Packets;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable, DataContract]
public class InitialTimeData
{
    [DataMember(Order = 1)]
    public TimeChange TimePacket;

    [DataMember(Order = 2)]
    public CrashedShipExploderData CrashedShipExploderData;

    [DataMember(Order = 3)]
    public SunbeamData SunbeamData;

    [IgnoreConstructor]
    protected InitialTimeData()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public InitialTimeData(TimeChange timePacket, CrashedShipExploderData crashedShipExploderData, SunbeamData sunbeamData)
    {
        TimePacket = timePacket;
        CrashedShipExploderData = crashedShipExploderData;
        SunbeamData = sunbeamData;
    }
}

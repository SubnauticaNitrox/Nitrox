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

    [IgnoreConstructor]
    protected InitialTimeData()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public InitialTimeData(TimeChange timePacket, CrashedShipExploderData crashedShipExploderData)
    {
        TimePacket = timePacket;
        CrashedShipExploderData = crashedShipExploderData;
    }
}

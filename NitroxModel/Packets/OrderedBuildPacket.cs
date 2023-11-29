using System;

namespace NitroxModel.Packets;

[Serializable]
public abstract class OrderedBuildPacket : Packet
{
    public int OperationId { get; set; }

    protected OrderedBuildPacket(int operationId)
    {
        OperationId = operationId;
    }
}

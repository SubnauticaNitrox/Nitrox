using System;

namespace NitroxModel.Networking.Packets;

[Serializable]
public abstract record OrderedBuildPacket : Packet
{
    public int OperationId { get; set; }

    protected OrderedBuildPacket(int operationId)
    {
        OperationId = operationId;
    }
}

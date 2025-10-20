using System;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public abstract class OrderedBuildPacket : Packet
{
    public int OperationId { get; set; }

    protected OrderedBuildPacket(int operationId)
    {
        OperationId = operationId;
    }
}

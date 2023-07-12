namespace NitroxModel.Packets;

public abstract class OrderedBuildPacket : Packet
{
    public int OperationId;

    public OrderedBuildPacket(int operationId)
    {
        OperationId = operationId;
    }
}

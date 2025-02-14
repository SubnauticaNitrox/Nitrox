using System;

namespace NitroxModel.Packets;

[Serializable]
public class JoinQueueInfo : Packet
{
    public int Position { get; }
    public int Timeout { get; }

    public JoinQueueInfo(int position, int timeout)
    {
        Position = position;
        Timeout = timeout;
    }
}

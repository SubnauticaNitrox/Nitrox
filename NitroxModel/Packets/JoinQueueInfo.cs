using System;

namespace NitroxModel.Packets;

[Serializable]
public class JoinQueueInfo : Packet
{
    public int Position { get; }
    public int Timeout { get; }
    public bool ShowMaximumWait { get; }

    public JoinQueueInfo(int position, int timeout, bool showMaximumWait)
    {
        Position = position;
        Timeout = timeout;
        ShowMaximumWait = showMaximumWait;
    }
}

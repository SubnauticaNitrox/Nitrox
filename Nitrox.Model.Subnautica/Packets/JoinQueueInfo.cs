using System;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

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

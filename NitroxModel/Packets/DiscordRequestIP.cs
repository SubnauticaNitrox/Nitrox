using System;

namespace NitroxModel.Packets;

[Serializable]
public class DiscordRequestIP : Packet
{
    public string IpPort { get; set; }

    public DiscordRequestIP(string ipPort)
    {
        IpPort = ipPort;
    }
}

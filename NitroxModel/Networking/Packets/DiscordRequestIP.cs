using System;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record DiscordRequestIP : Packet
{
    public string IpPort { get; set; }

    public DiscordRequestIP(string ipPort)
    {
        IpPort = ipPort;
    }
}

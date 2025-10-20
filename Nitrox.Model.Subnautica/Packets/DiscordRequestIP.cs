using System;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class DiscordRequestIP : Packet
{
    public string IpPort { get; set; }

    public DiscordRequestIP(string ipPort)
    {
        IpPort = ipPort;
    }
}

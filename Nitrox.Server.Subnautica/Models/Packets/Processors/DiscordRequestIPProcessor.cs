using System.Collections.Generic;
using System.Net;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class DiscordRequestIPProcessor : AuthenticatedPacketProcessor<DiscordRequestIP>
{
    private readonly IOptions<SubnauticaServerOptions> options;
    private readonly ILogger<DiscordRequestIPProcessor> logger;

    private string ipPort;

    public DiscordRequestIPProcessor(IOptions<SubnauticaServerOptions> options, ILogger<DiscordRequestIPProcessor> logger)
    {
        this.options = options;
        this.logger = logger;
    }

    public override void Process(DiscordRequestIP packet, Player player)
    {
        if (string.IsNullOrEmpty(ipPort))
        {
            Task.Run(() => ProcessPacketAsync(packet, player));
            return;
        }

        packet.IpPort = ipPort;
        player.SendPacket(packet);
    }

    private async Task ProcessPacketAsync(DiscordRequestIP packet, Player player)
    {
        string result = await GetIpAsync();
        if (result == "")
        {
            logger.ZLogError($"Couldn't get external Ip for discord request.");
            return;
        }

        packet.IpPort = ipPort = $"{result}:{options.Value.ServerPort}";
        player.SendPacket(packet);
    }

    /// <summary>
    /// Get the WAN IP address or the VPN IP address if the WAN IP address is not available.
    /// </summary>
    /// <returns>Found IP or blank string if none found</returns>
    private static async Task<string> GetIpAsync()
    {
        Task<IPAddress> wanIpTask = NetHelper.GetWanIpAsync();
        Task<IEnumerable<(IPAddress Address, string NetworkName)>> vpnIpsTask = Task.Run(NetHelper.GetVpnIps);
        if (await wanIpTask is {} wanIp)
        {
            return wanIp.ToString();
        }
        foreach ((IPAddress vpnAddress, string _) in await vpnIpsTask)
        {
            return vpnAddress.ToString();
        }

        return "";
    }
}

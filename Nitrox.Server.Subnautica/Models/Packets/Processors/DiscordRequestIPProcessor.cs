using System.Collections.Generic;
using System.Net;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class DiscordRequestIPProcessor(IOptions<SubnauticaServerOptions> options, ILogger<DiscordRequestIPProcessor> logger) : IAuthPacketProcessor<DiscordRequestIP>
{
    private readonly IOptions<SubnauticaServerOptions> options = options;
    private readonly ILogger<DiscordRequestIPProcessor> logger = logger;

    private string ipPort;

    public async Task Process(AuthProcessorContext context, DiscordRequestIP packet)
    {
        if (string.IsNullOrEmpty(ipPort))
        {
            await ProcessPacketAsync(context, packet);
            return;
        }

        packet.IpPort = ipPort;
        await context.ReplyAsync(packet);
    }

    private async Task ProcessPacketAsync(AuthProcessorContext context, DiscordRequestIP packet)
    {
        string result = await GetIpAsync();
        if (result == "")
        {
            logger.ZLogError($"Couldn't get external Ip for discord request.");
            return;
        }

        packet.IpPort = ipPort = $"{result}:{options.Value.ServerPort}";
        await context.ReplyAsync(packet);
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

using System.Net;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Helper;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class DiscordRequestIPProcessor(IOptions<SubnauticaServerOptions> optionsProvider, ILogger<DiscordRequestIPProcessor> logger) : IAuthPacketProcessor<DiscordRequestIP>
{
    private readonly IOptions<SubnauticaServerOptions> optionsProvider = optionsProvider;
    private readonly ILogger<DiscordRequestIPProcessor> logger = logger;

    private string ipPort;

    public async Task Process(AuthProcessorContext context, DiscordRequestIP packet)
    {
        if (string.IsNullOrEmpty(ipPort))
        {
            string result = await GetIpAsync();
            if (result == "")
            {
                logger.ZLogError($"Couldn't provide server external IP to session #{context.Sender.SessionId}");
                return;
            }

            packet.IpPort = ipPort = $"{result}:{optionsProvider.Value.ServerPort}";
            await context.ReplyToSender(packet);
        }

        packet.IpPort = ipPort;
        await context.ReplyToSender(packet);
    }

    /// <summary>
    ///     Get the WAN IP address or the Hamachi IP address if the WAN IP address is not available.
    /// </summary>
    /// <returns>Found IP or blank string if none found</returns>
    private static async Task<string> GetIpAsync()
    {
        IPAddress wanIp = await NetHelper.GetWanIpAsync();
        if (wanIp != null)
        {
            return wanIp.ToString();
        }
        IPAddress hamachiIp = await Task.Run(NetHelper.GetHamachiIp);
        if (hamachiIp != null)
        {
            return hamachiIp.ToString();
        }
        return "";
    }
}

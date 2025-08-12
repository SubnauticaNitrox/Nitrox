using System;
using System.Net;
using System.Threading.Tasks;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel.Serialization;
using NitroxServer.Communication.Packets.Processors.Abstract;

namespace NitroxServer.Communication.Packets.Processors;

public class DiscordRequestIPProcessor : AuthenticatedPacketProcessor<DiscordRequestIP>
{
    private readonly SubnauticaServerConfig serverConfig;

    private string ipPort;

    public DiscordRequestIPProcessor(SubnauticaServerConfig serverConfig)
    {
        this.serverConfig = serverConfig;
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
            Log.Error("Couldn't get external Ip for discord request.");
            return;
        }

        packet.IpPort = ipPort = $"{result}:{serverConfig.ServerPort}";
        player.SendPacket(packet);
    }

    /// <summary>
    /// Get the WAN IP address or the Hamachi IP address if the WAN IP address is not available.
    /// </summary>
    /// <returns>Found IP or blank string if none found</returns>
    private static async Task<string> GetIpAsync()
    {
        Task<IPAddress> wanIp = NetHelper.GetWanIpAsync();
        Task<IPAddress> hamachiIp = Task.Run(NetHelper.GetHamachiIp);
        if (await wanIp != null)
        {
            return wanIp.Result.ToString();
        }

        if (await hamachiIp != null)
        {
            return hamachiIp.Result.ToString();
        }
        return "";
    }
}

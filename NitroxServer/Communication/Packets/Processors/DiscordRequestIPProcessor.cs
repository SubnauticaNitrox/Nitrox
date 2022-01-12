using System.Net;
using System.Threading.Tasks;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.Serialization;

namespace NitroxServer.Communication.Packets.Processors;

public class DiscordRequestIPProcessor : AuthenticatedPacketProcessor<DiscordRequestIP>
{
    private readonly ServerConfig serverConfig;

    private string ipPort;

    public DiscordRequestIPProcessor(ServerConfig serverConfig)
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
        Task<IPAddress> wanIp = NetHelper.GetWanIpAsync();

        if (await wanIp == null)
        {
            Log.Error("Couldn't get external Ip for discord request.");
            return;
        }

        packet.IpPort = ipPort = $"{wanIp.Result}:{serverConfig.ServerPort}";
        player.SendPacket(packet);
    }
}

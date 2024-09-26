using System.Net;
using System.Threading.Tasks;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours.Discord;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class DiscordRequestIPProcessor : ClientPacketProcessor<DiscordRequestIP>
{
    public override async void Process(DiscordRequestIP packet)
    {
        string ipPort = await GetIpPortAsync();
        if (ipPort == "") return;
        DiscordClient.UpdateIpPort(ipPort);
    }

    /// <summary>
    /// Get the WAN IP address or the Hamachi IP address if the WAN IP address is not available.
    /// </summary>
    /// <returns>Found IP or blank string if none found</returns>
    private static async Task<string> GetIpPortAsync()
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

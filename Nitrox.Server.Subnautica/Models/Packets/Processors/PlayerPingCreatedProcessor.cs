using System.Threading.Tasks;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerPingCreatedProcessor : IAuthPacketProcessor<PlayerPingCreated>
{
    public async Task Process(AuthProcessorContext context, PlayerPingCreated packet)
    {
        await context.SendToOthersAsync(packet);
    }
}

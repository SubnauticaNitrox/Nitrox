using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class MapRoomCameraLightChangedProcessor : IAuthPacketProcessor<MapRoomCameraLightChanged>
{
    public async Task Process(AuthProcessorContext context, MapRoomCameraLightChanged packet)
    {
        await context.SendToOthersAsync(packet);
    }
}

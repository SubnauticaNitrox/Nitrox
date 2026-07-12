using System.Threading.Tasks;
using Nitrox.Model.Packets.Core;
using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class MapRoomCameraLightProcessor : IAuthPacketProcessor<MapRoomCameraLight>, IAuthPacketProcessor, IPacketProcessor, IPacketProcessor<AuthProcessorContext, MapRoomCameraLight>
{
	public async Task Process(AuthProcessorContext context, MapRoomCameraLight packet)
	{
		await context.SendToOthersAsync<MapRoomCameraLight>(packet);
	}
}



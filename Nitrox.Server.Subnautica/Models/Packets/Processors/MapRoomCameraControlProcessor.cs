using System.Threading.Tasks;
using Nitrox.Model.Packets.Core;
using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class MapRoomCameraControlProcessor : IAuthPacketProcessor<MapRoomCameraControl>, IAuthPacketProcessor, IPacketProcessor, IPacketProcessor<AuthProcessorContext, MapRoomCameraControl>
{
	public async Task Process(AuthProcessorContext context, MapRoomCameraControl packet)
	{
		await context.SendToOthersAsync<MapRoomCameraControl>(packet);
	}
}



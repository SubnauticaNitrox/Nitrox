using System.Threading.Tasks;
using Nitrox.Model.Packets.Core;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class MapRoomCameraControlProcessor(MapRoomCameras mapRoomCameras) : IClientPacketProcessor<MapRoomCameraControl>, IClientPacketProcessor, IPacketProcessor, IPacketProcessor<ClientProcessorContext, MapRoomCameraControl>
{
	private readonly MapRoomCameras mapRoomCameras = mapRoomCameras;

	public Task Process(ClientProcessorContext context, MapRoomCameraControl packet)
	{
		mapRoomCameras.ProcessControl(packet);
		return Task.CompletedTask;
	}
}



using System.Threading.Tasks;
using Nitrox.Model.Packets.Core;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class MapRoomCameraDockProcessor(MapRoomCameras mapRoomCameras) : IClientPacketProcessor<MapRoomCameraDock>, IClientPacketProcessor, IPacketProcessor, IPacketProcessor<ClientProcessorContext, MapRoomCameraDock>
{
	private readonly MapRoomCameras mapRoomCameras = mapRoomCameras;

	public Task Process(ClientProcessorContext context, MapRoomCameraDock packet)
	{
		mapRoomCameras.ProcessDock(packet);
		return Task.CompletedTask;
	}
}



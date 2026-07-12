using System.Threading.Tasks;
using Nitrox.Model.Packets.Core;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class MapRoomCameraLightProcessor(MapRoomCameras mapRoomCameras) : IClientPacketProcessor<MapRoomCameraLight>, IClientPacketProcessor, IPacketProcessor, IPacketProcessor<ClientProcessorContext, MapRoomCameraLight>
{
	private readonly MapRoomCameras mapRoomCameras = mapRoomCameras;

	public Task Process(ClientProcessorContext context, MapRoomCameraLight packet)
	{
		mapRoomCameras.ProcessLight(packet);
		return Task.CompletedTask;
	}
}



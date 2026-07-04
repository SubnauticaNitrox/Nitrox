using Nitrox.Model.Subnautica.DataStructures.GameLogic.Bases;
using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class MapRoomCameraNumberChangedProcessor(ILogger<MapRoomCameraNumberChangedProcessor> logger) : IAuthPacketProcessor<MapRoomCameraNumberChanged>
{
    private readonly ILogger<MapRoomCameraNumberChangedProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, MapRoomCameraNumberChanged packet)
    {
        if (packet.CameraId == null)
        {
            return;
        }

        if (packet.CameraNumber == 0)
        {
            MapRoomCameraRegistry.DeregisterCamera(packet.CameraId);

            logger.ZLogInformation($"Scanner camera registry released: cameraId={packet.CameraId}, registry=[{MapRoomCameraRegistry.GetDebugText()}]");

            await context.SendToAllAsync(new MapRoomCameraNumberChanged(packet.CameraId, 0));
            return;
        }

        MapRoomCameraRegistryEntry entry = MapRoomCameraRegistry.RegisterOrUpdateCamera(packet.CameraId, packet.MapRoomId, packet.DockingIndex);

        logger.ZLogInformation($"Scanner camera registry assigned: cameraId={entry.CameraId}, number={entry.CameraNumber}, mapRoom={entry.MapRoomId}, dockingIndex={entry.DockingIndex}, registry=[{MapRoomCameraRegistry.GetDebugText()}]");

        await context.SendToAllAsync(new MapRoomCameraNumberChanged(
            entry.CameraId,
            entry.CameraNumber,
            entry.MapRoomId,
            entry.DockingIndex));
    }
}

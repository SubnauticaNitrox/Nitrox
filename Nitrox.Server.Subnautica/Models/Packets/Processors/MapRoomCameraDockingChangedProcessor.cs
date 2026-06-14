using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;
using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class MapRoomCameraDockingChangedProcessor(EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, ILogger<MapRoomCameraDockingChangedProcessor> logger) : IAuthPacketProcessor<MapRoomCameraDockingChanged>
{
    private const string MAP_ROOM_CAMERA_PREFAB_CLASS_ID = "733fd479-0760-4bc2-a03e-281cbf02bfa4";
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly ILogger<MapRoomCameraDockingChangedProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, MapRoomCameraDockingChanged packet)
    {
        if (!entityRegistry.TryGetEntityById(packet.MapRoomId, out MapRoomEntity mapRoomEntity))
        {
            logger.ZLogError($"Unable to find scanner room {packet.MapRoomId} for camera docking update");
            return;
        }

        if (packet.DockingIndex < 0)
        {
            logger.ZLogWarning($"Ignoring scanner room camera docking update with invalid index {packet.DockingIndex} for {packet.MapRoomId}");
            return;
        }

        while (mapRoomEntity.CameraDockingStates.Count <= packet.DockingIndex)
        {
            mapRoomEntity.CameraDockingStates.Add(true);
        }

        while (mapRoomEntity.CameraDockingIds.Count <= packet.DockingIndex)
        {
            mapRoomEntity.CameraDockingIds.Add(null);
        }

        mapRoomEntity.CameraDockingStates[packet.DockingIndex] = packet.CameraDocked;
        mapRoomEntity.CameraDockingIds[packet.DockingIndex] = packet.CameraDocked ? packet.CameraId : null;

        if (packet.CameraId != null)
        {
            if (packet.CameraDocked)
            {
                MapRoomCameraRegistry.RegisterDockedCamera(packet.CameraId, packet.MapRoomId, packet.DockingIndex);
            }
            else
            {
                MapRoomCameraRegistry.MarkCameraUndocked(packet.CameraId);
                EnsureUndockedMapRoomCameraWorldEntityExists(packet.CameraId, mapRoomEntity);
            }
        }

        logger.ZLogInformation($"Scanner room camera docking state updated: mapRoom={packet.MapRoomId}, index={packet.DockingIndex}, docked={packet.CameraDocked}, cameraId={packet.CameraId}, states=[{string.Join(", ", mapRoomEntity.CameraDockingStates)}], cameraIds=[{string.Join(", ", mapRoomEntity.CameraDockingIds)}]");

        await context.SendToOthersAsync(packet);
    }

    private void EnsureUndockedMapRoomCameraWorldEntityExists(NitroxId cameraId, MapRoomEntity mapRoomEntity)
    {
        if (cameraId == null)
        {
            return;
        }

        if (entityRegistry.TryGetEntityById<WorldEntity>(cameraId, out WorldEntity existingWorldEntity))
        {
            logger.ZLogInformation($"Scanner room camera already registered as WorldEntity on undock: cameraId={cameraId}, entityType={existingWorldEntity.GetType().Name}, classId={existingWorldEntity.ClassId}, techType={existingWorldEntity.TechType}, level={existingWorldEntity.Level}, parentId={existingWorldEntity.ParentId}");
            return;
        }

        if (entityRegistry.TryGetEntityById<Entity>(cameraId, out Entity existingEntity))
        {
            logger.ZLogWarning($"Scanner room camera id exists in entity registry but is not a WorldEntity; registering world entity anyway: cameraId={cameraId}, existingType={existingEntity.GetType().Name}");
        }

        NitroxTransform transform = mapRoomEntity.Transform;

        GlobalRootEntity cameraEntity = new(
            transform,
            GlobalRootEntity.GLOBAL_ROOT_LEVEL,
            MAP_ROOM_CAMERA_PREFAB_CLASS_ID,
            true,
            cameraId,
            TechType.MapRoomCamera.ToDto(),
            null,
            null,
            new List<Entity>());

        logger.ZLogInformation($"Registering legacy undocked scanner room camera world entity before AddOrUpdateGlobalRootEntity: cameraId={cameraId}, mapRoom={mapRoomEntity.Id}, classId={MAP_ROOM_CAMERA_PREFAB_CLASS_ID}, level={GlobalRootEntity.GLOBAL_ROOT_LEVEL}");

        worldEntityManager.AddOrUpdateGlobalRootEntity(cameraEntity);

        logger.ZLogInformation($"Registered legacy undocked scanner room camera world entity: cameraId={cameraId}, mapRoom={mapRoomEntity.Id}, classId={MAP_ROOM_CAMERA_PREFAB_CLASS_ID}, level={GlobalRootEntity.GLOBAL_ROOT_LEVEL}");
    }
}

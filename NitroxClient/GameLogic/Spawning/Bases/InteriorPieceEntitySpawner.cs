using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Bases;

public class InteriorPieceEntitySpawner : EntitySpawner<InteriorPieceEntity>
{
    private readonly Entities entities;
    private readonly EntityMetadataManager entityMetadataManager;

    public InteriorPieceEntitySpawner(Entities entities, EntityMetadataManager entityMetadataManager)
    {
        this.entities = entities;
        this.entityMetadataManager = entityMetadataManager;
    }

    protected override IEnumerator SpawnAsync(InteriorPieceEntity entity, TaskResult<Optional<GameObject>> result)
    {
        if (entity.ParentId == null || !NitroxEntity.TryGetComponentFrom(entity.ParentId, out Base @base))
        {
            Log.Error($"Couldn't find a Base component on the parent object of InteriorPieceEntity {entity.Id}");
            yield break;
        }
        yield return RestoreInteriorPiece(entity, @base, result);
        if (!result.Get().HasValue)
        {
            Log.Error($"Restoring interior piece failed: {entity}");
            yield break;
        }
        bool isWaterPark = entity.IsWaterPark;

        List<Entity> batch = new();
        foreach (Entity childEntity in entity.ChildEntities)
        {
            switch(childEntity)
            {
                case InventoryItemEntity:
                case InstalledModuleEntity:
                    batch.Add(childEntity);
                    break;

                case PlanterEntity:
                    foreach (InventoryItemEntity childItemEntity in childEntity.ChildEntities.OfType<InventoryItemEntity>())
                    {
                        batch.Add(childItemEntity);
                    }
                    break;

                case WorldEntity:
                    if (isWaterPark)
                    {
                        batch.Add(childEntity);
                    }
                    break;
            }
        }

        if (isWaterPark)
        {
            // Must happen before child plant spawning
            foreach (Planter planter in result.Get().Value.GetComponentsInChildren<Planter>(true))
            {
                yield return planter.DeserializeAsync();
            }
        }

        yield return entities.SpawnBatchAsync(batch, true);

        if (result.Get().Value.TryGetComponent(out PowerSource powerSource))
        {
            // TODO: Have synced/restored power
            powerSource.SetPower(powerSource.maxPower);
        }
    }

    protected override bool SpawnsOwnChildren(InteriorPieceEntity entity) => true;

    public IEnumerator RestoreInteriorPiece(InteriorPieceEntity interiorPiece, Base @base, TaskResult<Optional<GameObject>> result = null)
    {
        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, classId: interiorPiece.ClassId))
        {
            TaskResult<GameObject> prefabResult = new();
            yield return DefaultWorldEntitySpawner.RequestPrefab(interiorPiece.ClassId, prefabResult);
            if (!prefabResult.Get())
            {
                Log.Error($"Couldn't find a prefab for interior piece of ClassId {interiorPiece.ClassId}");
                yield break;
            }
            prefab = prefabResult.Get();
        }

        Base.Face face = interiorPiece.BaseFace.ToUnity();
        face.cell += @base.GetAnchor();
        GameObject moduleObject = @base.SpawnModule(prefab, face);
        if (moduleObject)
        {
            NitroxEntity.SetNewId(moduleObject, interiorPiece.Id);
            yield return BuildingPostSpawner.ApplyPostSpawner(moduleObject, interiorPiece.Id);
            entityMetadataManager.ApplyMetadata(moduleObject, interiorPiece.Metadata);
            result.Set(moduleObject);
        }
    }

    public static InteriorPieceEntity From(IBaseModule module, EntityMetadataManager entityMetadataManager)
    {
        InteriorPieceEntity interiorPiece = InteriorPieceEntity.MakeEmpty();
        interiorPiece.Level = (int)LargeWorldEntity.CellLevel.Global;

        GameObject gameObject = (module as Component).gameObject;
        if (gameObject && gameObject.TryGetComponent(out PrefabIdentifier identifier))
        {
            interiorPiece.ClassId = identifier.ClassId;
        }
        else
        {
            Log.Warn($"Couldn't find an identifier for the interior piece {module.GetType()}");
        }

        if (gameObject.TryGetIdOrWarn(out NitroxId entityId))
        {
            interiorPiece.Id = entityId;
        }

        if (gameObject.TryGetComponentInParent(out Base parentBase, true) &&
            parentBase.TryGetNitroxId(out NitroxId parentId))
        {
            interiorPiece.ParentId = parentId;
        }

        switch (module)
        {
            case LargeRoomWaterPark:
                PlanterEntity leftPlanter = new(interiorPiece.Id.Increment(), interiorPiece.Id);
                PlanterEntity rightPlanter = new(leftPlanter.Id.Increment(), interiorPiece.Id);
                interiorPiece.ChildEntities.Add(leftPlanter);
                interiorPiece.ChildEntities.Add(rightPlanter);
                break;
            // When you deconstruct (not entirely) then construct back those pieces, they keep their inventories
            case BaseNuclearReactor baseNuclearReactor:
                interiorPiece.ChildEntities.AddRange(Items.GetEquipmentModuleEntities(baseNuclearReactor.equipment, entityId, entityMetadataManager));
                break;
            case BaseBioReactor baseBioReactor:
                foreach (ItemsContainer.ItemGroup itemGroup in baseBioReactor.container._items.Values)
                {
                    foreach (InventoryItem item in itemGroup.items)
                    {
                        interiorPiece.ChildEntities.Add(Items.ConvertToInventoryItemEntity(item.item.gameObject, interiorPiece.Id, entityMetadataManager));
                    }
                }
                break;
            case WaterPark:
                PlanterEntity planter = new(interiorPiece.Id.Increment(), interiorPiece.Id);
                interiorPiece.ChildEntities.Add(planter);
                break;
        }

        interiorPiece.BaseFace = module.moduleFace.ToDto();

        return interiorPiece;
    }

    public static IEnumerator RestoreMapRoom(Base @base, MapRoomEntity mapRoomEntity, Entities entities)
    {
        MapRoomFunctionality mapRoomFunctionality = @base.GetMapRoomFunctionalityForCell(mapRoomEntity.Cell.ToUnity());
        if (!mapRoomFunctionality)
        {
            Log.Error($"Couldn't find MapRoomFunctionality in base for cell {mapRoomEntity.Cell}");
            yield break;
        }

        NitroxEntity.SetNewId(mapRoomFunctionality.gameObject, mapRoomEntity.Id);

        yield return RestoreMapRoomCameraDockingStates(mapRoomFunctionality, mapRoomEntity);

        mapRoomFunctionality.StartCoroutine(AdoptExistingDockedMapRoomCamerasWhenReady(mapRoomFunctionality, mapRoomEntity.Id));

        List<Entity> upgradeChips = mapRoomEntity.ChildEntities
                                                 .OfType<InventoryItemEntity>()
                                                 .ToList<Entity>();

        if (upgradeChips.Count > 0)
        {
            ClearMapRoomUpgradeContainer(mapRoomFunctionality);
            yield return entities.SpawnBatchAsync(upgradeChips, true);
        }
    }

    private static IEnumerator AdoptExistingDockedMapRoomCamerasWhenReady(MapRoomFunctionality mapRoomFunctionality, NitroxId mapRoomId)
    {
        for (int attempt = 0; attempt < 900; attempt++)
        {
            if (!mapRoomFunctionality)
            {
                Log.Warn($"SCANNER_CAMERA_ADOPT_ABORT mapRoom={mapRoomId} reason=mapRoomFunctionalityMissing attempt={attempt}");
                yield break;
            }

            List<MapRoomCameraDocking> dockings = mapRoomFunctionality.GetComponentsInChildren<MapRoomCameraDocking>(true)
                                                                      .OrderBy(docking => docking.gameObject.GetFullHierarchyPath())
                                                                      .ToList();

            bool adoptedAnyCamera = false;

            for (int dockingIndex = 0; dockingIndex < dockings.Count; dockingIndex++)
            {
                MapRoomCameraDocking docking = dockings[dockingIndex];
                MapRoomCamera camera = docking.camera;

                if (!CanAdoptExistingDockedMapRoomCamera(docking, camera, out string rejectReason))
                {
                    continue;
                }

                if (!camera.TryGetNitroxId(out NitroxId cameraId) || cameraId == null)
                {
                    cameraId = GetLegacyDockedMapRoomCameraId(mapRoomId, dockingIndex);
                    NitroxEntity.SetNewId(camera.gameObject, cameraId);

                    Log.Warn($"SCANNER_CAMERA_ADOPT_ASSIGNED_LEGACY_ID mapRoom={mapRoomId} attempt={attempt} index={dockingIndex} cameraId={cameraId} camera={camera.gameObject.GetFullHierarchyPath()}");
                }

                using (PacketSuppressor<MapRoomCameraDockingChanged>.Suppress())
                {
                    docking.camera = null;
                    docking.cameraDocked = false;

                    if (camera.dockingPoint == docking)
                    {
                        camera.dockingPoint = null;
                    }

                    docking.DockCamera(camera);
                    NormalizeRestoredDockedMapRoomCamera(camera, docking);
                }

                yield return EnsureLegacyDockedCameraHasBattery(camera);

                NormalizeRestoredDockedMapRoomCamera(camera, docking);

                if (!MapRoomCamera.cameras.Contains(camera))
                {
                    MapRoomCamera.cameras.Add(camera);
                }

                NitroxServiceLocator.LocateService<IPacketSender>()
                                    .Send(new MapRoomCameraDockingChanged(mapRoomId, dockingIndex, true, cameraId));

                MapRoomCameraIdentity.RequestCameraNumber(camera, mapRoomId, dockingIndex);
                MapRoomCameraIdentity.NormalizeCameraNumbers();

                camera.UpdatePingLabel();
                adoptedAnyCamera = true;
            }

            if (adoptedAnyCamera)
            {
                yield break;
            }

            yield return null;
        }

        Log.Warn($"SCANNER_CAMERA_ADOPT_TIMEOUT mapRoom={mapRoomId}");
    }

    private static IEnumerator EnsureLegacyDockedCameraHasBattery(MapRoomCamera camera)
    {
        if (!camera)
        {
            yield break;
        }

        EnergyMixin energyMixin = camera.GetComponent<EnergyMixin>();
        if (!energyMixin)
        {
            Log.Warn($"SCANNER_CAMERA_BATTERY_SKIP reason=missingEnergyMixin camera={camera.gameObject.GetFullHierarchyPath()}");
            yield break;
        }

        if (energyMixin.battery != null)
        {
            yield break;
        }

        TechType batteryTechType = energyMixin.defaultBattery != TechType.None
            ? energyMixin.defaultBattery
            : TechType.Battery;

        GameObject batteryPrefab;
        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out batteryPrefab, batteryTechType))
        {
            TaskResult<GameObject> prefabResult = new();
            yield return DefaultWorldEntitySpawner.RequestPrefab(batteryTechType, prefabResult);

            batteryPrefab = prefabResult.Get();
            if (!batteryPrefab)
            {
                Log.Warn($"SCANNER_CAMERA_BATTERY_SKIP reason=missingBatteryPrefab techType={batteryTechType} camera={camera.gameObject.GetFullHierarchyPath()}");
                yield break;
            }
        }

        GameObject batteryObject = UnityEngine.Object.Instantiate(batteryPrefab);
        batteryObject.SetActive(true);

        if (camera.TryGetNitroxId(out NitroxId cameraId) && cameraId != null)
        {
            NitroxEntity.SetNewId(batteryObject, GetMapRoomCameraBatteryId(cameraId));
        }

        Pickupable pickupable = batteryObject.GetComponent<Pickupable>();
        Battery battery = batteryObject.GetComponent<Battery>();

        if (!pickupable || !battery)
        {
            Log.Warn($"SCANNER_CAMERA_BATTERY_SKIP reason=spawnedBatteryMissingComponents techType={batteryTechType} camera={camera.gameObject.GetFullHierarchyPath()}");
            UnityEngine.Object.Destroy(batteryObject);
            yield break;
        }

        energyMixin.batterySlot.AddItem(new InventoryItem(pickupable));

        if (energyMixin.capacity > 0f)
        {
            energyMixin.AddEnergy(energyMixin.capacity);
        }
        else if (battery.capacity > 0f)
        {
            battery.charge = battery.capacity;
        }
    }

    private static bool CanAdoptExistingDockedMapRoomCamera(MapRoomCameraDocking docking, MapRoomCamera camera, out string rejectReason)
    {
        if (!docking)
        {
            rejectReason = "dockingMissing";
            return false;
        }

        if (!camera)
        {
            rejectReason = "cameraMissing";
            return false;
        }

        if (docking.camera != camera)
        {
            rejectReason = "dockingCameraMismatch";
            return false;
        }

        if (camera.dockingPoint != docking)
        {
            rejectReason = "cameraDockingPointMismatch";
            return false;
        }

        EnergyMixin energyMixin = camera.GetComponent<EnergyMixin>();
        if (!energyMixin)
        {
            rejectReason = "missingEnergyMixin";
            return false;
        }

        Pickupable pickupable = camera.GetComponent<Pickupable>();
        if (!pickupable)
        {
            rejectReason = "missingPickupable";
            return false;
        }

        rejectReason = "";
        return true;
    }

    private static NitroxId GetMapRoomCameraBatteryId(NitroxId cameraId)
    {
        using MD5 md5 = MD5.Create();

        byte[] inputBytes = Encoding.UTF8.GetBytes($"map-room-camera-battery:{cameraId}");
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        byte[] guidBytes = new byte[16];
        Array.Copy(hashBytes, guidBytes, guidBytes.Length);

        return new NitroxId(new Guid(guidBytes));
    }

    private static NitroxId GetLegacyDockedMapRoomCameraId(NitroxId mapRoomId, int dockingIndex)
    {
        using MD5 md5 = MD5.Create();

        byte[] inputBytes = Encoding.UTF8.GetBytes($"legacy-map-room-camera:{mapRoomId}:{dockingIndex}");
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        byte[] guidBytes = new byte[16];
        Array.Copy(hashBytes, guidBytes, guidBytes.Length);

        return new NitroxId(new Guid(guidBytes));
    }

    private static IEnumerator RestoreMapRoomCameraDockingStates(MapRoomFunctionality mapRoomFunctionality, MapRoomEntity mapRoomEntity)
    {
        if ((mapRoomEntity.CameraDockingIds == null || mapRoomEntity.CameraDockingIds.Count == 0) &&
            (mapRoomEntity.CameraDockingStates == null || mapRoomEntity.CameraDockingStates.Count == 0))
        {
            yield break;
        }

        List<MapRoomCameraDocking> dockings = mapRoomFunctionality.GetComponentsInChildren<MapRoomCameraDocking>(true)
                                                                  .OrderBy(docking => docking.gameObject.GetFullHierarchyPath())
                                                                  .ToList();

        int count = System.Math.Min(
            dockings.Count,
            System.Math.Max(mapRoomEntity.CameraDockingIds?.Count ?? 0, mapRoomEntity.CameraDockingStates?.Count ?? 0));

        for (int dockingIndex = 0; dockingIndex < count; dockingIndex++)
        {
            MapRoomCameraDocking docking = dockings[dockingIndex];

            bool cameraDocked = mapRoomEntity.CameraDockingStates != null &&
                                dockingIndex < mapRoomEntity.CameraDockingStates.Count &&
                                mapRoomEntity.CameraDockingStates[dockingIndex];

            NitroxId cameraId = mapRoomEntity.CameraDockingIds != null &&
                                dockingIndex < mapRoomEntity.CameraDockingIds.Count
                ? mapRoomEntity.CameraDockingIds[dockingIndex]
                : null;

            if (!cameraDocked)
            {
                NitroxId legacyUndockedCameraId = GetLegacyDockedMapRoomCameraId(mapRoomEntity.Id, dockingIndex);

                if (TryPromoteLegacyUndockedMapRoomCameraFromDockSlot(
                        docking,
                        legacyUndockedCameraId,
                        mapRoomEntity.Id,
                        dockingIndex))
                {
                    continue;
                }

                ClearMapRoomCameraDockingSlot(docking);
                continue;
            }

            if (cameraId == null)
            {
                Log.Warn($"Clearing legacy scanner room docked camera with no persisted camera id. mapRoom={mapRoomEntity.Id}, dockingIndex={dockingIndex}, slot={docking.gameObject.GetFullHierarchyPath()}");
                ClearMapRoomCameraDockingSlot(docking);
                continue;
            }

            yield return RestoreSingleDockedMapRoomCamera(docking, cameraId, mapRoomEntity.Id, dockingIndex);
        }
    }

    private static IEnumerator RestoreSingleDockedMapRoomCamera(MapRoomCameraDocking docking, NitroxId cameraId, NitroxId mapRoomId, int dockingIndex)
    {
        if (cameraId == null)
        {
            ClearMapRoomCameraDockingSlot(docking);
            yield break;
        }

        docking.StartCoroutine(DockPersistedMapRoomCameraWhenAvailable(docking, cameraId, mapRoomId, dockingIndex));
        yield break;
    }

    private static IEnumerator DockPersistedMapRoomCameraWhenAvailable(MapRoomCameraDocking docking, NitroxId cameraId, NitroxId mapRoomId, int dockingIndex)
    {
        GameObject cameraObject = null;

        // This runs as its own coroutine so it does not block the base/entity restore pipeline.
        // Persisted camera world entities can appear several seconds after the map room itself.
        for (int attempt = 0; attempt < 3600; attempt++)
        {
            if (!docking)
            {
                yield break;
            }

            if (NitroxEntity.TryGetObjectFrom(cameraId, out cameraObject) && cameraObject)
            {
                MapRoomCamera camera = cameraObject.GetComponent<MapRoomCamera>();
                if (!camera)
                {
                    Log.Warn($"Persisted scanner room camera {cameraId} did not have MapRoomCamera: {cameraObject.GetFullHierarchyPath()}");
                    yield break;
                }

                ClearMapRoomCameraDockingSlot(docking);

                using (PacketSuppressor<MapRoomCameraDockingChanged>.Suppress())
                {
                    docking.DockCamera(camera);
                    NormalizeRestoredDockedMapRoomCamera(camera, docking);
                }

                MapRoomCameraIdentity.RequestCameraNumber(camera, mapRoomId, dockingIndex);

                Log.Info($"Restored persisted scanner room camera {cameraId} into docking slot {docking.gameObject.GetFullHierarchyPath()}");
                yield break;
            }

            MapRoomCamera legacyDockedCamera = docking.camera;
            if (legacyDockedCamera &&
                legacyDockedCamera.gameObject &&
                !legacyDockedCamera.TryGetNitroxId(out _) &&
                cameraId.Equals(GetLegacyDockedMapRoomCameraId(mapRoomId, dockingIndex)))
            {
                NitroxEntity.SetNewId(legacyDockedCamera.gameObject, cameraId);
                NormalizeRestoredDockedMapRoomCamera(legacyDockedCamera, docking);
                MapRoomCameraIdentity.RequestCameraNumber(legacyDockedCamera, mapRoomId, dockingIndex);

                Log.Info($"Restored persisted legacy scanner room camera {cameraId} by assigning saved id to existing docked camera in slot {docking.gameObject.GetFullHierarchyPath()}");
                yield break;
            }

            yield return null;
        }

        Log.Warn($"Could not find persisted scanner room camera {cameraId} while restoring docking slot {docking.gameObject.GetFullHierarchyPath()} after deferred retry");
    }

    private static void NormalizeRestoredDockedMapRoomCamera(MapRoomCamera camera, MapRoomCameraDocking docking)
    {
        camera.transform.SetPositionAndRotation(docking.transform.position, docking.transform.rotation);

        camera.SetDocked(docking);

        camera.readyForControl = true;
        camera.justStartedControl = false;

        if (camera.rigidBody)
        {
            camera.rigidBody.velocity = Vector3.zero;
            camera.rigidBody.angularVelocity = Vector3.zero;
            camera.rigidBody.isKinematic = true;
            camera.rigidBody.position = docking.transform.position;
            camera.rigidBody.rotation = docking.transform.rotation;
        }
    }

    private static bool TryPromoteLegacyUndockedMapRoomCameraFromDockSlot(
    MapRoomCameraDocking docking,
    NitroxId cameraId,
    NitroxId mapRoomId,
    int dockingIndex)
    {
        if (!docking)
        {
            return false;
        }

        MapRoomCamera camera = docking.camera;
        if (!camera || !camera.gameObject)
        {
            return false;
        }

        if (NitroxEntity.TryGetObjectFrom(cameraId, out GameObject existingObject) &&
            existingObject &&
            existingObject != camera.gameObject)
        {
            Log.Warn($"SCANNER_CAMERA_UNDOCKED_LEGACY_PROMOTE_REPLACE_EXISTING mapRoom={mapRoomId} dock={dockingIndex} cameraId={cameraId} existing={existingObject.GetFullHierarchyPath()} replacement={camera.gameObject.GetFullHierarchyPath()}");

            NitroxEntity.RemoveFrom(existingObject);
            UnityEngine.Object.Destroy(existingObject);
        }

        NitroxEntity.SetNewId(camera.gameObject, cameraId);

        using (PacketSuppressor<MapRoomCameraDockingChanged>.Suppress())
        {
            docking.UndockCamera();

            if (camera.dockingPoint == docking)
            {
                camera.dockingPoint = null;
            }

            docking.camera = null;
            docking.cameraDocked = false;
            camera.SetDocked(null);
        }

        camera.readyForControl = true;
        camera.justStartedControl = false;

        if (camera.rigidBody)
        {
            camera.rigidBody.isKinematic = false;
            camera.rigidBody.velocity = Vector3.zero;
            camera.rigidBody.angularVelocity = Vector3.zero;
            camera.rigidBody.WakeUp();
        }

        if (!MapRoomCamera.cameras.Contains(camera))
        {
            MapRoomCamera.cameras.Add(camera);
        }

        MapRoomCameraIdentity.RequestCameraNumber(camera, null, -1);
        MapRoomCameraIdentity.NormalizeCameraNumbers();
        camera.UpdatePingLabel();

        Log.Warn($"SCANNER_CAMERA_UNDOCKED_LEGACY_PROMOTED_FROM_DOCK_SLOT mapRoom={mapRoomId} dock={dockingIndex} cameraId={cameraId} camera={camera.gameObject.GetFullHierarchyPath()} position={camera.transform.position} ready={camera.IsReady()} readyForControl={camera.readyForControl} cameraListCount={MapRoomCamera.cameras.Count}");

        return true;
    }

    private static void ClearMapRoomCameraDockingSlot(MapRoomCameraDocking docking)
    {
        if (docking.camera)
        {
            Log.Info($"Clearing scanner room docking slot camera {docking.camera.gameObject.GetFullHierarchyPath()} number={docking.camera.GetCameraNumber()}");

            NitroxEntity.RemoveFrom(docking.camera.gameObject);
            UnityEngine.Object.Destroy(docking.camera.gameObject);
        }

        docking.camera = null;
        docking.cameraDocked = false;
    }

    private static void ClearMapRoomUpgradeContainer(MapRoomFunctionality mapRoomFunctionality)
    {
        Optional<ItemsContainer> opContainer = InventoryContainerHelper.TryGetContainerByOwner(mapRoomFunctionality.gameObject);

        if (!opContainer.HasValue)
        {
            Log.Error($"Couldn't find map room upgrade container on {mapRoomFunctionality.gameObject.GetFullHierarchyPath()}");
            return;
        }

        ItemsContainer container = opContainer.Value;

        List<Pickupable> pickupables = container._items.Values
                                              .SelectMany(itemGroup => itemGroup.items)
                                              .Select(item => item.item)
                                              .Where(pickupable => pickupable)
                                              .ToList();

        foreach (Pickupable pickupable in pickupables)
        {
            NitroxEntity.RemoveFrom(pickupable.gameObject);
            container.RemoveItem(pickupable, true);
        }
    }

}

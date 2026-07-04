using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxClient.MonoBehaviours;
using UWE;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public sealed class MapRoomCameraNumberChangedProcessor : IClientPacketProcessor<MapRoomCameraNumberChanged>
{
    private static readonly HashSet<NitroxId> batteryEnsureInProgress = [];

    public Task Process(ClientProcessorContext context, MapRoomCameraNumberChanged packet)
    {
        MapRoomCameraIdentity.ApplyCameraNumber(packet.CameraId, packet.CameraNumber);

        if (packet.CameraId != null && packet.CameraNumber > 0)
        {
            CoroutineHost.StartCoroutine(ApplyCameraNumberWhenCameraExists(packet));
        }

        return Task.CompletedTask;
    }

    private static IEnumerator ApplyCameraNumberWhenCameraExists(MapRoomCameraNumberChanged packet)
    {
        for (int attempt = 0; attempt < 300; attempt++)
        {
            if (TryApplyLiveCameraNumber(packet, attempt))
            {
                yield break;
            }

            yield return null;
        }

        Log.Warn($"SCANNER_CAMERA_NUMBER_APPLY_TIMEOUT cameraId={packet.CameraId} number={packet.CameraNumber} cameraListCount={MapRoomCamera.cameras.Count}");
    }

    private static bool TryApplyLiveCameraNumber(MapRoomCameraNumberChanged packet, int attempt)
    {
        if (packet.CameraId == null || packet.CameraNumber <= 0)
        {
            return true;
        }

        if (!NitroxEntity.TryGetObjectFrom(packet.CameraId, out GameObject cameraObject) || !cameraObject)
        {
            return false;
        }

        MapRoomCamera camera = cameraObject.GetComponent<MapRoomCamera>();
        if (!camera)
        {
            Log.Warn($"SCANNER_CAMERA_NUMBER_TARGET_NOT_CAMERA cameraId={packet.CameraId} object={cameraObject.GetFullHierarchyPath()} number={packet.CameraNumber}");
            return true;
        }

        if (!IsActiveWorldCamera(camera))
        {
            return false;
        }

        MapRoomCamera.cameras.RemoveAll(candidate => !candidate);

        if (!MapRoomCamera.cameras.Contains(camera))
        {
            MapRoomCamera.cameras.Add(camera);
        }

        if (camera.cameraNumber != packet.CameraNumber)
        {
            camera.cameraNumber = packet.CameraNumber;
            camera.UpdatePingLabel();
        }

        camera.readyForControl = true;

        if (!batteryEnsureInProgress.Contains(packet.CameraId))
        {
            batteryEnsureInProgress.Add(packet.CameraId);
            CoroutineHost.StartCoroutine(EnsureMapRoomCameraHasBattery(camera, packet.CameraId));
        }

        MapRoomCameraIdentity.NormalizeCameraNumbers();

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

    private static IEnumerator EnsureMapRoomCameraHasBattery(MapRoomCamera camera, NitroxId cameraId)
    {
        try
        {
            if (!camera)
            {
                yield break;
            }

            EnergyMixin energyMixin = camera.GetComponent<EnergyMixin>();
            if (!energyMixin)
            {
                Log.Warn($"SCANNER_CAMERA_NUMBER_BATTERY_SKIP reason=missingEnergyMixin cameraId={cameraId} camera={camera.gameObject.GetFullHierarchyPath()}");
                yield break;
            }

            if (energyMixin.battery != null)
            {
                if (energyMixin.capacity > 0f)
                {
                    energyMixin.AddEnergy(energyMixin.capacity);
                }

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
                    Log.Warn($"SCANNER_CAMERA_NUMBER_BATTERY_SKIP reason=missingBatteryPrefab cameraId={cameraId} techType={batteryTechType} camera={camera.gameObject.GetFullHierarchyPath()}");
                    yield break;
                }
            }

            GameObject batteryObject = UnityEngine.Object.Instantiate(batteryPrefab);
            batteryObject.SetActive(true);

            if (cameraId != null)
            {
                NitroxEntity.SetNewId(batteryObject, GetMapRoomCameraBatteryId(cameraId));
            }

            Pickupable pickupable = batteryObject.GetComponent<Pickupable>();
            Battery battery = batteryObject.GetComponent<Battery>();

            if (!pickupable || !battery)
            {
                Log.Warn($"SCANNER_CAMERA_NUMBER_BATTERY_SKIP reason=spawnedBatteryMissingComponents cameraId={cameraId} techType={batteryTechType} camera={camera.gameObject.GetFullHierarchyPath()}");

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

            camera.readyForControl = true;
        }
        finally
        {
            batteryEnsureInProgress.Remove(cameraId);
        }
    }

    private static bool IsActiveWorldCamera(MapRoomCamera camera)
    {
        if (!camera)
        {
            return false;
        }

        if (camera.pickupAble && camera.pickupAble.attached)
        {
            return false;
        }

        return camera.isActiveAndEnabled &&
               !camera.gameObject.GetFullHierarchyPath().Contains("/Inventory/");
    }
}

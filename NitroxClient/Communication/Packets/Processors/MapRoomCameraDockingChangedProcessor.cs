using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours;
using NitroxClient.GameLogic.Bases;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public sealed class MapRoomCameraDockingChangedProcessor : IClientPacketProcessor<MapRoomCameraDockingChanged>
{
    public Task Process(ClientProcessorContext context, MapRoomCameraDockingChanged packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.MapRoomId, out GameObject mapRoomObject))
        {
            Log.Warn($"Could not find scanner room for camera docking update: {packet.MapRoomId}");
            return Task.CompletedTask;
        }

        MapRoomFunctionality mapRoomFunctionality = mapRoomObject.GetComponent<MapRoomFunctionality>();
        if (!mapRoomFunctionality)
        {
            Log.Warn($"Could not find MapRoomFunctionality on scanner room docking update target: {mapRoomObject.GetFullHierarchyPath()}");
            return Task.CompletedTask;
        }

        List<MapRoomCameraDocking> dockings = mapRoomFunctionality.GetComponentsInChildren<MapRoomCameraDocking>(true)
                                                                  .OrderBy(docking => docking.gameObject.GetFullHierarchyPath())
                                                                  .ToList();

        if (packet.DockingIndex < 0 || packet.DockingIndex >= dockings.Count)
        {
            Log.Warn($"Scanner room camera docking index out of range: {packet.DockingIndex}, count={dockings.Count}");
            return Task.CompletedTask;
        }

        using (PacketSuppressor<MapRoomCameraDockingChanged>.Suppress())
        {
            ApplyDockingState(dockings[packet.DockingIndex], packet);
        }

        MapRoomCameraIdentity.NormalizeCameraNumbers();

        return Task.CompletedTask;
    }

    private static void ApplyDockingState(MapRoomCameraDocking docking, MapRoomCameraDockingChanged packet)
    {
        if (!packet.CameraDocked)
        {
            ApplyLiveUndockState(docking, packet);
            return;
        }

        if (packet.CameraId == null)
        {
            docking.cameraDocked = true;
            return;
        }

        if (!NitroxEntity.TryGetObjectFrom(packet.CameraId, out GameObject cameraObject))
        {
            Log.Warn($"Could not find scanner room camera {packet.CameraId} for docking update");
            docking.cameraDocked = true;
            return;
        }

        MapRoomCamera camera = cameraObject.GetComponent<MapRoomCamera>();
        if (!camera)
        {
            Log.Warn($"Scanner room camera docking update target did not have MapRoomCamera: {cameraObject.GetFullHierarchyPath()}");
            docking.cameraDocked = true;
            return;
        }

        using (PacketSuppressor<MapRoomCameraDockingChanged>.Suppress())
        {
            if (docking.camera && docking.camera != camera)
            {
                ClearDockingSlot(docking);
            }

            if (docking.camera != camera)
            {
                docking.DockCamera(camera);
            }

            NormalizeDockedMapRoomCamera(camera, docking);
        }

        MapRoomCameraIdentity.NormalizeCameraNumbers();
    }

    private static void NormalizeDockedMapRoomCamera(MapRoomCamera camera, MapRoomCameraDocking docking)
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

    private static void ApplyLiveUndockState(MapRoomCameraDocking docking, MapRoomCameraDockingChanged packet)
    {
        // Old/restore-style empty slot update with no specific camera id:
        // clear any local placeholder/default dock camera.
        if (packet.CameraId == null)
        {
            ClearDockingSlot(docking);
            return;
        }

        if (!NitroxEntity.TryGetObjectFrom(packet.CameraId, out GameObject cameraObject))
        {
            Log.Warn($"Could not find scanner room camera {packet.CameraId} for undocking update");
            docking.camera = null;
            docking.cameraDocked = false;
            return;
        }

        MapRoomCamera camera = cameraObject.GetComponent<MapRoomCamera>();
        if (!camera)
        {
            Log.Warn($"Scanner room camera undocking update target did not have MapRoomCamera: {cameraObject.GetFullHierarchyPath()}");
            docking.camera = null;
            docking.cameraDocked = false;
            return;
        }

        using (PacketSuppressor<MapRoomCameraDockingChanged>.Suppress())
        {
            if (docking.camera == camera)
            {
                docking.UndockCamera();
            }
            else
            {
                camera.transform.position = docking.undockTransform.position;
                camera.transform.rotation = docking.undockTransform.rotation;
                camera.SetDocked(null);

                docking.camera = null;
                docking.cameraDocked = false;
            }
        }

        if (camera.rigidBody)
        {
            camera.rigidBody.isKinematic = false;
            camera.rigidBody.velocity = Vector3.zero;
            camera.rigidBody.angularVelocity = Vector3.zero;
        }
    }

    private static void ClearDockingSlot(MapRoomCameraDocking docking)
    {
        if (docking.camera)
        {
            NitroxEntity.RemoveFrom(docking.camera.gameObject);
            Object.Destroy(docking.camera.gameObject);
        }

        docking.camera = null;
        docking.cameraDocked = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases;

public static class MapRoomCameraIdentity
{
    private static readonly Dictionary<NitroxId, int> cameraNumbersById = [];

    public static void ApplyCameraNumber(NitroxId cameraId, int cameraNumber)
    {
        if (cameraId == null)
        {
            return;
        }

        if (cameraNumber <= 0)
        {
            cameraNumbersById.Remove(cameraId);
            RemoveCameraFromScannerRegistry(cameraId);
            return;
        }

        cameraNumbersById[cameraId] = cameraNumber;

        ApplyCameraNumberToLiveCamera(cameraId, cameraNumber);
        NormalizeCameraNumbers();
    }

    private static void ApplyCameraNumberToLiveCamera(NitroxId cameraId, int cameraNumber)
    {
        if (!NitroxEntity.TryGetObjectFrom(cameraId, out GameObject cameraObject) || !cameraObject)
        {
            return;
        }

        MapRoomCamera camera = cameraObject.GetComponent<MapRoomCamera>();
        if (!camera || !IsActiveWorldCamera(camera))
        {
            return;
        }

        MapRoomCamera.cameras.RemoveAll(candidate => !candidate);

        if (!MapRoomCamera.cameras.Contains(camera))
        {
            MapRoomCamera.cameras.Add(camera);
        }

        if (camera.cameraNumber != cameraNumber)
        {
            camera.cameraNumber = cameraNumber;
            camera.UpdatePingLabel();
        }
    }

    private static void RemoveCameraFromScannerRegistry(NitroxId cameraId)
    {
        MapRoomCamera camera = MapRoomCamera.cameras
            .FirstOrDefault(candidate => candidate &&
                                         candidate.TryGetNitroxId(out NitroxId candidateId) &&
                                         candidateId == cameraId);

        if (!camera)
        {
            return;
        }

        MapRoomCamera.cameras.Remove(camera);

        if (camera.dockingPoint && camera.dockingPoint.camera == camera)
        {
            camera.dockingPoint.camera = null;
            camera.dockingPoint.cameraDocked = false;
        }

        camera.SetDocked(null);
        camera.UpdatePingLabel();
    }

    public static void AssignCameraNumber(MapRoomCamera camera, int cameraNumber)
    {
        if (!camera || cameraNumber <= 0)
        {
            return;
        }

        if (!camera.TryGetNitroxId(out NitroxId cameraId) || cameraId == null)
        {
            return;
        }

        cameraNumbersById[cameraId] = cameraNumber;

        if (camera.cameraNumber != cameraNumber)
        {
            camera.cameraNumber = cameraNumber;
            camera.UpdatePingLabel();
        }

        NormalizeCameraNumbers();
    }

    public static void RequestCameraNumber(MapRoomCamera camera, NitroxId mapRoomId = null, int dockingIndex = -1)
    {
        if (!IsActiveWorldCamera(camera))
        {
            return;
        }

        if (!camera.TryGetNitroxId(out NitroxId cameraId) || cameraId == null)
        {
            return;
        }

        NitroxServiceLocator.LocateService<IPacketSender>()
                            .Send(new MapRoomCameraNumberChanged(cameraId, -1, mapRoomId, dockingIndex));
    }

    public static void ReleaseCameraNumber(MapRoomCamera camera)
    {
        if (!camera || !camera.TryGetNitroxId(out NitroxId cameraId) || cameraId == null)
        {
            return;
        }

        cameraNumbersById.Remove(cameraId);
        RemoveCameraFromScannerRegistry(cameraId);

        if (camera.cameraNumber != 0)
        {
            camera.cameraNumber = 0;
            camera.UpdatePingLabel();
        }

        NitroxServiceLocator.LocateService<IPacketSender>()
                            .Send(new MapRoomCameraNumberChanged(cameraId, 0));

        NormalizeCameraNumbers();
    }

    public static IEnumerator RequestCameraNumberWhenReady(MapRoomCamera camera)
    {
        for (int attempt = 0; attempt < 180; attempt++)
        {
            if (!camera)
            {
                yield break;
            }

            if (IsActiveWorldCamera(camera) && camera.TryGetNitroxId(out NitroxId cameraId) && cameraId != null)
            {
                cameraNumbersById.Remove(cameraId);
                NitroxServiceLocator.LocateService<IPacketSender>()
                                    .Send(new MapRoomCameraNumberChanged(cameraId, -1));
                yield break;
            }

            yield return null;
        }
    }

    public static void NormalizeCameraNumbers()
    {
        Dictionary<NitroxId, MapRoomCamera> camerasById = [];

        foreach (MapRoomCamera camera in MapRoomCamera.cameras.ToList())
        {
            if (!camera || !IsActiveWorldCamera(camera))
            {
                continue;
            }

            if (!camera.TryGetNitroxId(out NitroxId cameraId) || cameraId == null)
            {
                continue;
            }

            if (!cameraNumbersById.TryGetValue(cameraId, out int assignedNumber) || assignedNumber <= 0)
            {
                continue;
            }

            if (!camerasById.TryGetValue(cameraId, out MapRoomCamera existingCamera) ||
                ShouldPreferCamera(camera, existingCamera))
            {
                camerasById[cameraId] = camera;
            }
        }

        MapRoomCamera.cameras.Clear();
        MapRoomCamera.cameras.AddRange(camerasById.Values);

        foreach (MapRoomCamera camera in MapRoomCamera.cameras)
        {
            if (!camera.TryGetNitroxId(out NitroxId cameraId) || cameraId == null)
            {
                continue;
            }

            if (!cameraNumbersById.TryGetValue(cameraId, out int assignedNumber) || assignedNumber <= 0)
            {
                continue;
            }

            if (camera.cameraNumber != assignedNumber)
            {
                camera.cameraNumber = assignedNumber;
                camera.UpdatePingLabel();
            }
        }

        MapRoomCamera.cameras.Sort(CompareCamerasByServerNumber);
    }

    public static void NotifyCameraPickedUp(MapRoomCamera camera)
    {
        if (!camera)
        {
            return;
        }

        NotifyDockingSlotClearedForPickedUpCamera(camera);
        ReleaseCameraNumber(camera);
    }

    private static void NotifyDockingSlotClearedForPickedUpCamera(MapRoomCamera camera)
    {
        if (!camera.TryGetNitroxId(out NitroxId cameraId))
        {
            cameraId = null;
        }

        MapRoomCameraDocking docking = camera.dockingPoint;
        if (!docking)
        {
            return;
        }

        MapRoomFunctionality mapRoomFunctionality = docking.GetComponentInParent<MapRoomFunctionality>(true);
        if (!mapRoomFunctionality)
        {
            return;
        }

        if (!mapRoomFunctionality.TryGetNitroxId(out NitroxId mapRoomId) || mapRoomId == null)
        {
            return;
        }

        List<MapRoomCameraDocking> dockings = mapRoomFunctionality.GetComponentsInChildren<MapRoomCameraDocking>(true)
                                                                  .OrderBy(candidate => candidate.gameObject.GetFullHierarchyPath())
                                                                  .ToList();

        int dockingIndex = dockings.IndexOf(docking);
        if (dockingIndex < 0)
        {
            return;
        }

        NitroxServiceLocator.LocateService<IPacketSender>()
                            .Send(new MapRoomCameraDockingChanged(mapRoomId, dockingIndex, false, cameraId));
    }

    private static bool ShouldPreferCamera(MapRoomCamera candidate, MapRoomCamera existing)
    {
        if (!existing)
        {
            return true;
        }

        if (!candidate)
        {
            return false;
        }

        bool candidateDocked = candidate.dockingPoint && candidate.dockingPoint.camera == candidate;
        bool existingDocked = existing.dockingPoint && existing.dockingPoint.camera == existing;

        if (candidateDocked != existingDocked)
        {
            return candidateDocked;
        }

        if (candidate.isActiveAndEnabled != existing.isActiveAndEnabled)
        {
            return candidate.isActiveAndEnabled;
        }

        return false;
    }

    private static int CompareCamerasByServerNumber(MapRoomCamera left, MapRoomCamera right)
    {
        int leftNumber = GetAssignedCameraNumberOrMax(left);
        int rightNumber = GetAssignedCameraNumberOrMax(right);

        int numberComparison = leftNumber.CompareTo(rightNumber);
        if (numberComparison != 0)
        {
            return numberComparison;
        }

        return string.CompareOrdinal(TryGetCameraIdText(left), TryGetCameraIdText(right));
    }

    private static int GetAssignedCameraNumberOrMax(MapRoomCamera camera)
    {
        if (!camera || !camera.TryGetNitroxId(out NitroxId cameraId) || cameraId == null)
        {
            return int.MaxValue;
        }

        return cameraNumbersById.TryGetValue(cameraId, out int cameraNumber) && cameraNumber > 0
            ? cameraNumber
            : int.MaxValue;
    }

    private static string TryGetCameraIdText(MapRoomCamera camera)
    {
        if (!camera || !camera.TryGetNitroxId(out NitroxId cameraId) || cameraId == null)
        {
            return string.Empty;
        }

        return cameraId.ToString();
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

        return camera.isActiveAndEnabled;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Logger;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.Extensions;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.GameLogic;

public class MapRoomCameras
{
	private readonly IPacketSender packetSender;

	private readonly Dictionary<NitroxId, bool> lastBroadcastLightState = new Dictionary<NitroxId, bool>();

	private readonly HashSet<NitroxId> locallyControlled = new HashSet<NitroxId>();

	public MapRoomCameras(IPacketSender packetSender)
	{
		this.packetSender = packetSender;
	}

	public void BroadcastControl(MapRoomCamera camera, bool isControlling)
	{
		if (!camera)
		{
			return;
		}
		NitroxId nitroxId2;
		if (isControlling)
		{
			Optional<NitroxId> mapRoomId = Optional.Empty;
			int cameraIndex = -1;
			MapRoomFunctionality mapRoomForDock = GetMapRoomForDock(camera.dockingPoint);
			if ((bool)mapRoomForDock && mapRoomForDock.TryGetNitroxId(out NitroxId nitroxId))
			{
				mapRoomId = Optional.Of(nitroxId);
				cameraIndex = GetDockingIndex(mapRoomForDock, camera.dockingPoint);
			}
			NitroxId idOrGenerateNew = NitroxEntity.GetIdOrGenerateNew(camera.gameObject);
			bool flag = (bool)camera.lightsParent && camera.lightsParent.activeSelf;
			lastBroadcastLightState[idOrGenerateNew] = flag;
			locallyControlled.Add(idOrGenerateNew);
			MovementBroadcaster.RegisterWatched(camera.gameObject, idOrGenerateNew);
			packetSender.Send(new MapRoomCameraControl(idOrGenerateNew, mapRoomId, cameraIndex, isControlling: true, flag));
		}
		else if (camera.TryGetNitroxId(out nitroxId2))
		{
			MovementBroadcaster.UnregisterWatched(nitroxId2);
			lastBroadcastLightState.Remove(nitroxId2);
			packetSender.Send(new MapRoomCameraControl(nitroxId2, Optional.Empty, -1, isControlling: false, lightOn: false));
		}
	}

	public void ProcessControl(MapRoomCameraControl packet)
	{
		GameObject gameObject2;
		MapRoomCameraMovementReplicator component;
		if (packet.IsControlling)
		{
			GameObject gameObject = ResolveCameraObject(packet.CameraId, packet.MapRoomId, packet.CameraIndex);
			if (!gameObject)
			{
				Log.Warn(string.Format("[{0}] Couldn't find a camera drone to replicate for {1}", "MapRoomCameras", packet));
				return;
			}
			SetLight(gameObject, packet.LightOn);
			if (!gameObject.GetComponent<MapRoomCameraMovementReplicator>())
			{
				gameObject.AddComponent<MapRoomCameraMovementReplicator>();
			}
		}
		else if (NitroxEntity.TryGetObjectFrom(packet.CameraId, out gameObject2) && gameObject2.TryGetComponent<MapRoomCameraMovementReplicator>(out component))
		{
			UnityEngine.Object.Destroy(component);
		}
	}

	public void BroadcastLightIfChanged(MapRoomCamera camera)
	{
		if ((bool)camera && (bool)camera.lightsParent && camera.TryGetNitroxId(out NitroxId nitroxId))
		{
			bool activeSelf = camera.lightsParent.activeSelf;
			if (!lastBroadcastLightState.TryGetValue(nitroxId, out var value) || value != activeSelf)
			{
				lastBroadcastLightState[nitroxId] = activeSelf;
				packetSender.Send(new MapRoomCameraLight(nitroxId, activeSelf));
			}
		}
	}

	public void ProcessLight(MapRoomCameraLight packet)
	{
		if (NitroxEntity.TryGetObjectFrom(packet.CameraId, out GameObject gameObject) && (bool)gameObject)
		{
			SetLight(gameObject, packet.On);
		}
	}

	public void BroadcastDock(MapRoomCameraDocking dockingPoint, MapRoomCamera camera)
	{
		if (!PacketSuppressor<MapRoomCameraDock>.IsSuppressed && (bool)dockingPoint && (bool)camera && camera.TryGetNitroxId(out NitroxId nitroxId) && locallyControlled.Remove(nitroxId))
		{
			MapRoomFunctionality mapRoomForDock = GetMapRoomForDock(dockingPoint);
			if ((bool)mapRoomForDock && mapRoomForDock.TryGetNitroxId(out NitroxId nitroxId2))
			{
				packetSender.Send(new MapRoomCameraDock(nitroxId, nitroxId2, GetDockingIndex(mapRoomForDock, dockingPoint)));
			}
		}
	}

	public void ProcessDock(MapRoomCameraDock packet)
	{
		if (!NitroxEntity.TryGetObjectFrom(packet.CameraId, out GameObject gameObject) || !gameObject || !gameObject.TryGetComponent<MapRoomCamera>(out var component) || !NitroxEntity.TryGetObjectFrom(packet.MapRoomId, out GameObject gameObject2) || !gameObject2 || !gameObject2.TryGetComponent<MapRoomFunctionality>(out var component2))
		{
			return;
		}
		List<MapRoomCameraDocking> dockingPoints = GetDockingPoints(component2);
		if (packet.DockingIndex < 0 || packet.DockingIndex >= dockingPoints.Count)
		{
			return;
		}
		if (gameObject.TryGetComponent<MapRoomCameraMovementReplicator>(out var component3))
		{
			UnityEngine.Object.Destroy(component3);
		}
		using (PacketSuppressor<MapRoomCameraDock>.Suppress())
		{
			dockingPoints[packet.DockingIndex].DockCamera(component);
		}
	}

	private static void SetLight(GameObject cameraObject, bool on)
	{
		if (cameraObject.TryGetComponent<MapRoomCamera>(out var component) && (bool)component.lightsParent && component.lightsParent.activeSelf != on)
		{
			component.lightsParent.SetActive(on);
		}
	}

	private static GameObject ResolveCameraObject(NitroxId cameraId, Optional<NitroxId> mapRoomId, int cameraIndex)
	{
		if (NitroxEntity.TryGetObjectFrom(cameraId, out GameObject gameObject) && (bool)gameObject)
		{
			return gameObject;
		}
		if (mapRoomId.HasValue && cameraIndex >= 0 && NitroxEntity.TryGetObjectFrom(mapRoomId.Value, out GameObject gameObject2) && (bool)gameObject2 && gameObject2.TryGetComponent<MapRoomFunctionality>(out var component))
		{
			List<MapRoomCameraDocking> dockingPoints = GetDockingPoints(component);
			if (cameraIndex < dockingPoints.Count)
			{
				MapRoomCamera camera = dockingPoints[cameraIndex].camera;
				if ((bool)camera)
				{
					NitroxEntity.SetNewId(camera.gameObject, cameraId);
					return camera.gameObject;
				}
			}
		}
		return null;
	}

	private static int GetDockingIndex(MapRoomFunctionality mapRoom, MapRoomCameraDocking dockingPoint)
	{
		List<MapRoomCameraDocking> dockingPoints = GetDockingPoints(mapRoom);
		for (int i = 0; i < dockingPoints.Count; i++)
		{
			if (dockingPoints[i] == dockingPoint)
			{
				return i;
			}
		}
		return -1;
	}

	private static NitroxId GetDeterministicCameraId(NitroxId mapRoomId, Vector3 localDockPosition)
	{
		int value = Mathf.RoundToInt(localDockPosition.x * 10f);
		int value2 = Mathf.RoundToInt(localDockPosition.y * 10f);
		int value3 = Mathf.RoundToInt(localDockPosition.z * 10f);
		byte[] array = new Guid(mapRoomId.ToString()).ToByteArray();
		byte[] bytes = BitConverter.GetBytes(value);
		byte[] bytes2 = BitConverter.GetBytes(value2);
		byte[] bytes3 = BitConverter.GetBytes(value3);
		for (int i = 0; i < 4; i++)
		{
			array[i] ^= bytes[i];
			array[i + 4] ^= bytes2[i];
			array[i + 8] ^= bytes3[i];
		}
		return new NitroxId(array);
	}

	private static Vector3 GetLocalDockPosition(MapRoomFunctionality mapRoom, MapRoomCameraDocking dockingPoint)
	{
		return mapRoom.transform.InverseTransformPoint(dockingPoint.transform.position);
	}

	public static void EnsureCameraIds(MapRoomFunctionality mapRoom)
	{
		if (!mapRoom || !mapRoom.TryGetNitroxId(out NitroxId nitroxId))
		{
			return;
		}
		List<MapRoomCameraDocking> dockingPoints = GetDockingPoints(mapRoom);
		int num = 0;
		foreach (MapRoomCameraDocking item in dockingPoints)
		{
			MapRoomCamera camera = item.camera;
			if ((bool)camera && !camera.TryGetNitroxId(out NitroxId _))
			{
				NitroxEntity.SetNewId(camera.gameObject, GetDeterministicCameraId(nitroxId, GetLocalDockPosition(mapRoom, item)));
				num++;
			}
		}
		Log.Info(string.Format("[{0}] EnsureCameraIds map room {1}: found {2} dock(s), assigned {3} camera id(s)", "MapRoomCameras", nitroxId, dockingPoints.Count, num));
	}

	public static IEnumerator EnsureCameraIdsDeferred(MapRoomFunctionality mapRoom)
	{
		float timeoutAt = Time.time + 15f;
		yield return new WaitUntil(() => !mapRoom || Time.time >= timeoutAt || MapRoomReadyForIds(mapRoom));
		EnsureCameraIds(mapRoom);
	}

	public static void EnsureCameraId(MapRoomCameraDocking dockingPoint, MapRoomCamera camera)
	{
		if ((bool)dockingPoint && (bool)camera && !camera.TryGetNitroxId(out NitroxId _))
		{
			MapRoomFunctionality mapRoomForDock = GetMapRoomForDock(dockingPoint);
			if ((bool)mapRoomForDock && mapRoomForDock.TryGetNitroxId(out NitroxId nitroxId2))
			{
				Vector3 localDockPosition = GetLocalDockPosition(mapRoomForDock, dockingPoint);
				NitroxId deterministicCameraId = GetDeterministicCameraId(nitroxId2, localDockPosition);
				NitroxEntity.SetNewId(camera.gameObject, deterministicCameraId);
				Log.Info(string.Format("[{0}] assigned camera id {1} (map room {2}, localPos {3:F2},{4:F2},{5:F2})", "MapRoomCameras", deterministicCameraId, nitroxId2, localDockPosition.x, localDockPosition.y, localDockPosition.z));
			}
		}
	}

	public static void DestroyStaleLocalCamera(NitroxId cameraId)
	{
		if (NitroxEntity.TryGetObjectFrom(cameraId, out GameObject gameObject) && (bool)gameObject && gameObject.TryGetComponent<MapRoomCamera>(out var _))
		{
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	private static bool MapRoomReadyForIds(MapRoomFunctionality mapRoom)
	{
		if (!mapRoom || !mapRoom.TryGetNitroxId(out NitroxId _))
		{
			return false;
		}
		foreach (MapRoomCameraDocking dockingPoint in GetDockingPoints(mapRoom))
		{
			if ((bool)dockingPoint.camera)
			{
				return true;
			}
		}
		return false;
	}

	private static List<MapRoomCameraDocking> GetDockingPoints(MapRoomFunctionality mapRoom)
	{
		List<MapRoomCameraDocking> list = new List<MapRoomCameraDocking>();
		if (!mapRoom)
		{
			return list;
		}
		Base componentInParent = mapRoom.GetComponentInParent<Base>();
		if ((bool)componentInParent)
		{
			MapRoomCameraDocking[] componentsInChildren = componentInParent.GetComponentsInChildren<MapRoomCameraDocking>(includeInactive: true);
			foreach (MapRoomCameraDocking mapRoomCameraDocking in componentsInChildren)
			{
				if ((bool)mapRoomCameraDocking && GetMapRoomForDock(mapRoomCameraDocking) == mapRoom)
				{
					list.Add(mapRoomCameraDocking);
				}
			}
		}
		if (list.Count == 0)
		{
			list.AddRange(mapRoom.GetComponentsInChildren<MapRoomCameraDocking>(includeInactive: true));
		}
		list.Sort((MapRoomCameraDocking a, MapRoomCameraDocking b) => CompareWorldPosition(a.transform.position, b.transform.position));
		return list;
	}

	private static MapRoomFunctionality GetMapRoomForDock(MapRoomCameraDocking dockingPoint)
	{
		if (!dockingPoint)
		{
			return null;
		}
		Base componentInParent = dockingPoint.GetComponentInParent<Base>();
		if ((bool)componentInParent)
		{
			MapRoomFunctionality mapRoomFunctionality = null;
			float num = float.MaxValue;
			Vector3 position = dockingPoint.transform.position;
			MapRoomFunctionality[] componentsInChildren = componentInParent.GetComponentsInChildren<MapRoomFunctionality>(includeInactive: true);
			foreach (MapRoomFunctionality mapRoomFunctionality2 in componentsInChildren)
			{
				float sqrMagnitude = (mapRoomFunctionality2.transform.position - position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					mapRoomFunctionality = mapRoomFunctionality2;
				}
			}
			if ((bool)mapRoomFunctionality)
			{
				return mapRoomFunctionality;
			}
		}
		return dockingPoint.GetComponentInParent<MapRoomFunctionality>();
	}

	private static int CompareWorldPosition(Vector3 a, Vector3 b)
	{
		if (Mathf.Abs(a.x - b.x) > 0.01f)
		{
			return a.x.CompareTo(b.x);
		}
		if (Mathf.Abs(a.y - b.y) > 0.01f)
		{
			return a.y.CompareTo(b.y);
		}
		return a.z.CompareTo(b.z);
	}
}



using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class MapRoomCameraDock : Packet
{
	public NitroxId CameraId { get; }

	public NitroxId MapRoomId { get; }

	public int DockingIndex { get; }

	public MapRoomCameraDock(NitroxId cameraId, NitroxId mapRoomId, int dockingIndex)
	{
		CameraId = cameraId;
		MapRoomId = mapRoomId;
		DockingIndex = dockingIndex;
	}

	public override string ToString()
	{
		return $"[MapRoomCameraDock - CameraId: {CameraId}, MapRoomId: {MapRoomId}, DockingIndex: {DockingIndex}]";
	}
}





using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class MapRoomCameraLight : Packet
{
	public NitroxId CameraId { get; }

	public bool On { get; }

	public MapRoomCameraLight(NitroxId cameraId, bool on)
	{
		CameraId = cameraId;
		On = on;
	}

	public override string ToString()
	{
		return $"[MapRoomCameraLight - CameraId: {CameraId}, On: {On}]";
	}
}





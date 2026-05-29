using System;
using BinaryPack.Attributes;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class MapRoomCameraNumberChanged : Packet
{
    public NitroxId CameraId { get; }
    public int CameraNumber { get; }
    public NitroxId MapRoomId { get; }
    public int DockingIndex { get; }

    [IgnoreConstructor]
    private MapRoomCameraNumberChanged()
    {
        // Constructor for serialization.
    }

    public MapRoomCameraNumberChanged(NitroxId cameraId, int cameraNumber, NitroxId mapRoomId = null, int dockingIndex = -1)
    {
        CameraId = cameraId;
        CameraNumber = cameraNumber;
        MapRoomId = mapRoomId;
        DockingIndex = dockingIndex;
    }
}

using System;
using BinaryPack.Attributes;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class MapRoomCameraDockingChanged : Packet
{
    public NitroxId MapRoomId { get; }
    public int DockingIndex { get; }
    public bool CameraDocked { get; }
    public NitroxId CameraId { get; }

    [IgnoreConstructor]
    private MapRoomCameraDockingChanged()
    {
        // Constructor for serialization.
    }

    public MapRoomCameraDockingChanged(NitroxId mapRoomId, int dockingIndex, bool cameraDocked, NitroxId cameraId = null)
    {
        MapRoomId = mapRoomId;
        DockingIndex = dockingIndex;
        CameraDocked = cameraDocked;
        CameraId = cameraId;
    }
}

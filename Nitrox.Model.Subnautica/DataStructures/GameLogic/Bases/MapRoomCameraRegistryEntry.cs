using System;
using System.Runtime.Serialization;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Bases;

[Serializable, DataContract]
public class MapRoomCameraRegistryEntry
{
    [DataMember(Order = 1)]
    public NitroxId CameraId { get; set; }

    [DataMember(Order = 2)]
    public int CameraNumber { get; set; }

    [DataMember(Order = 3)]
    public NitroxId MapRoomId { get; set; }

    [DataMember(Order = 4)]
    public int DockingIndex { get; set; } = -1;

    public MapRoomCameraRegistryEntry()
    {
        // Constructor for serialization.
    }

    public MapRoomCameraRegistryEntry(NitroxId cameraId, int cameraNumber, NitroxId mapRoomId = null, int dockingIndex = -1)
    {
        CameraId = cameraId;
        CameraNumber = cameraNumber;
        MapRoomId = mapRoomId;
        DockingIndex = dockingIndex;
    }

    public bool IsDocked => MapRoomId != null && DockingIndex >= 0;
}

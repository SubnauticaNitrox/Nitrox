using System;
using BinaryPack.Attributes;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class MapRoomCameraLightChanged : Packet
{
    public NitroxId CameraId { get; }
    public bool LightState { get; }

    [IgnoreConstructor]
    private MapRoomCameraLightChanged()
    {
        // Constructor for serialization.
    }

    public MapRoomCameraLightChanged(NitroxId cameraId, bool lightState)
    {
        CameraId = cameraId;
        LightState = lightState;
    }
}

using System;

namespace NitroxModel.Packets;

[Serializable]
public class UpdateDisplaySurfaceWater : Packet
{
    public bool DisplaySurfaceWater { get; }

    public UpdateDisplaySurfaceWater(bool displaySurfaceWater)
    {
        DisplaySurfaceWater = displaySurfaceWater;
    }
}

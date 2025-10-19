using System;

namespace Nitrox.Model.Packets;

[Serializable]
public class UpdateDisplaySurfaceWater : Packet
{
    public bool DisplaySurfaceWater { get; }

    public UpdateDisplaySurfaceWater(bool displaySurfaceWater)
    {
        DisplaySurfaceWater = displaySurfaceWater;
    }
}

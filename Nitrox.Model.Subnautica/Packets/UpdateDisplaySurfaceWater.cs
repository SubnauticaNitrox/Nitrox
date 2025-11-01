using System;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class UpdateDisplaySurfaceWater : Packet
{
    public bool DisplaySurfaceWater { get; }

    public UpdateDisplaySurfaceWater(bool displaySurfaceWater)
    {
        DisplaySurfaceWater = displaySurfaceWater;
    }
}

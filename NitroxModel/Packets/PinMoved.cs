using System;
using System.Collections.Generic;

namespace NitroxModel.Packets;

[Serializable]
public class PinMoved : Packet
{
    public List<int> Pins { get; }
    
    public PinMoved(List<int> pins)
    {
        Pins = pins;
    }
}

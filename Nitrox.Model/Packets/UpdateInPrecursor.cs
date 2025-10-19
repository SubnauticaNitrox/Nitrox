using System;

namespace Nitrox.Model.Packets;

[Serializable]
public class UpdateInPrecursor : Packet
{
    public bool InPrecursor { get; }

    public UpdateInPrecursor(bool inPrecursor)
    {
        InPrecursor = inPrecursor;
    }
}

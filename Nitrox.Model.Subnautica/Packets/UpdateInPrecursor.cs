using System;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class UpdateInPrecursor : Packet
{
    public bool InPrecursor { get; }

    public UpdateInPrecursor(bool inPrecursor)
    {
        InPrecursor = inPrecursor;
    }
}

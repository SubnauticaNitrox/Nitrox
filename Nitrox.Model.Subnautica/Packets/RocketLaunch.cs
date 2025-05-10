using System;
using NitroxModel.DataStructures;
using NitroxModel.Networking.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public record RocketLaunch : Packet
{
    public NitroxId RocketId { get; }

    public RocketLaunch(NitroxId rocketId)
    {
        RocketId = rocketId;
    }
}

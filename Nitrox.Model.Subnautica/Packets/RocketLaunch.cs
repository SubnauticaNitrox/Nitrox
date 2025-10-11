using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class RocketLaunch : Packet
{
    public NitroxId RocketId { get; }

    public RocketLaunch(NitroxId rocketId)
    {
        RocketId = rocketId;
    }
}

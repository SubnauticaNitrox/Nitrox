using System;
using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class RocketResync : Packet
{
    public NitroxId RocketId { get; }
    public List<PreflightCheck> PreflightChecks { get; }

    public RocketResync(NitroxId rocketId, List<PreflightCheck> preflightChecks)
    {
        RocketId = rocketId;
        PreflightChecks = preflightChecks;
    }
}

using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.Networking.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public record RocketResync : Packet
{
    public NitroxId RocketId { get; }
    public List<PreflightCheck> PreflightChecks { get; }

    public RocketResync(NitroxId rocketId, List<PreflightCheck> preflightChecks)
    {
        RocketId = rocketId;
        PreflightChecks = preflightChecks;
    }
}

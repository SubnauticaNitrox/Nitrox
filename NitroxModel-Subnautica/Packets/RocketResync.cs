using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets;

[Serializable]
public class RocketResync : Packet
{
    public NitroxId RocketId;
    public List<PreflightCheck> PreflightChecks;
    public RocketResync(NitroxId rocketId, List<PreflightCheck> preflightChecks)
    {
        RocketId = rocketId;
        PreflightChecks = preflightChecks;
    }
}

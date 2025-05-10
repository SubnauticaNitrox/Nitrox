using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;

namespace NitroxModel.Networking.Packets;

[Serializable]
public sealed record BuildingResync : Packet
{
    public Dictionary<BuildEntity, int> BuildEntities { get; }
    public Dictionary<ModuleEntity, int> ModuleEntities { get; }

    public BuildingResync(Dictionary<BuildEntity, int> buildEntities, Dictionary<ModuleEntity, int> moduleEntities)
    {
        BuildEntities = buildEntities;
        ModuleEntities = moduleEntities;
    }
}

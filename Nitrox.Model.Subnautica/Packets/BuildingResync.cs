using System;
using System.Collections.Generic;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class BuildingResync : Packet
{
    public Dictionary<BuildEntity, int> BuildEntities { get; }
    public Dictionary<ModuleEntity, int> ModuleEntities { get; }

    public BuildingResync(Dictionary<BuildEntity, int> buildEntities, Dictionary<ModuleEntity, int> moduleEntities)
    {
        BuildEntities = buildEntities;
        ModuleEntities = moduleEntities;
    }
}

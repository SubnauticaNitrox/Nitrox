using System;
using NitroxModel.DataStructures.GameLogic;
using System.Collections.Generic;

namespace NitroxModel.Packets;

[Serializable]
public class BuildingResync : Packet
{
    public Dictionary<Entity, int> Entities { get; }

    public BuildingResync(Dictionary<Entity, int> entities)
    {
        Entities = entities;
    }
}

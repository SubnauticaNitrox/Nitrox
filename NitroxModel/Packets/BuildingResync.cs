using NitroxModel.DataStructures.GameLogic;
using System.Collections.Generic;

namespace NitroxModel.Packets;

public class BuildingResync : Packet
{
    public Dictionary<Entity, int> Entities { get; set; }

    public BuildingResync(Dictionary<Entity, int> entities)
    {
        Entities = entities;
    }
}

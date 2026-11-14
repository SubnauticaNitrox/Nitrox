using System;
using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class SpawnEntities : Packet
{
    public List<Entity> Entities { get; }
    public List<SimulatedEntity> SimulatedEntities { get; }

    public List<AbsoluteEntityCell> SpawnedCells { get; }

    public bool ForceRespawn { get; }

    public SpawnEntities(Entity entity, SimulatedEntity? simulatedEntity = null, bool forceRespawn = false)
    {
        Entities = [entity];
        SimulatedEntities = [];
        SpawnedCells = [];
        if (simulatedEntity != null)
        {
            SimulatedEntities.Add(simulatedEntity);
        }

        ForceRespawn = forceRespawn;
    }

    // Constructor for serialization. 
    public SpawnEntities(List<Entity> entities, List<SimulatedEntity> simulatedEntities, List<AbsoluteEntityCell> spawnedCells, bool forceRespawn)
    {
        Entities = entities;
        SimulatedEntities = simulatedEntities;
        SpawnedCells = spawnedCells;
        ForceRespawn = forceRespawn;
    }
}

using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SpawnEntities : Packet
    {
        public List<Entity> Entities { get; }
        public List<SimulatedEntity> Simulations { get; }

        public List<AbsoluteEntityCell> SpawnedCells { get; }

        public bool ForceRespawn { get; }

        public SpawnEntities(List<Entity> entities, List<AbsoluteEntityCell> spawnedCells, bool forceRespawn = false)
        {
            Entities = entities;
            Simulations = [];
            SpawnedCells = spawnedCells;
            ForceRespawn = forceRespawn;
        }

        public SpawnEntities(Entity entity, SimulatedEntity simulatedEntity = null, bool forceRespawn = false)
        {
            Entities = [entity];
            Simulations = [];
            SpawnedCells = [];
            if (simulatedEntity != null)
            {
                Simulations.Add(simulatedEntity);
            }

            ForceRespawn = forceRespawn;
        }

        // Constructor for serialization. 
        public SpawnEntities(List<Entity> entities, List<SimulatedEntity> simulations, List<AbsoluteEntityCell> spawnedCells, bool forceRespawn)
        {
            Entities = entities;
            Simulations = simulations;
            SpawnedCells = spawnedCells;
            ForceRespawn = forceRespawn;
        }
    }
}

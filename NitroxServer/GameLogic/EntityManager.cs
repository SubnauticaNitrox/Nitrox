using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NitroxServer.GameLogic
{
    public class EntityManager
    {
        private Dictionary<AbsoluteEntityCell, List<SpawnedEntity>> entitiesByAbsoluteCell;
        private Dictionary<String, SpawnedEntity> entitiesByGuid;

        public EntityManager(EntitySpawner entitySpawner)
        {
            entitiesByAbsoluteCell = entitySpawner.GetEntitiesByAbsoluteCell();
            entitiesByGuid = entitiesByAbsoluteCell.Values.SelectMany(l => l).ToDictionary(e => e.Guid, e => e);
        }

        public List<SpawnedEntity> GetVisibleEntities(VisibleCell[] cells)
        {
            List<SpawnedEntity> entities = new List<SpawnedEntity>();

            foreach (VisibleCell cell in cells)
            {
                List<SpawnedEntity> cellEntities;

                if (entitiesByAbsoluteCell.TryGetValue(cell.AbsoluteCellEntity, out cellEntities))
                {
                    foreach (SpawnedEntity entity in cellEntities)
                    {
                        if (cell.Level <= entity.Level)
                        {
                            entities.Add(entity);
                        }
                    }
                }
            }

            return entities;
        }

        public void AllowEntitySimulationFor(String playerId, VisibleCell[] added)
        {
            foreach (VisibleCell cell in added)
            {
                List<SpawnedEntity> entities;

                if (entitiesByAbsoluteCell.TryGetValue(cell.AbsoluteCellEntity, out entities))
                {
                    foreach (SpawnedEntity entity in entities)
                    {
                        if (cell.Level <= entity.Level && entity.SimulatingPlayerId.IsEmpty())
                        {
                            entity.SimulatingPlayerId = Optional<String>.Of(playerId);
                        }
                    }
                }
            }
        }

        public void RevokeEntitySimulationFor(String playerId, VisibleCell[] removed)
        {
            foreach (VisibleCell cell in removed)
            {
                List<SpawnedEntity> entities;

                if (entitiesByAbsoluteCell.TryGetValue(cell.AbsoluteCellEntity, out entities))
                {
                    foreach (SpawnedEntity entity in entities)
                    {
                        if (entity.SimulatingPlayerId.IsPresent() && entity.SimulatingPlayerId.Equals(playerId))
                        {
                            entity.SimulatingPlayerId = Optional<String>.Empty();
                        }
                    }
                }
            }
        }
    }
}

using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.GameLogic;
using NitroxModel.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NitroxServer.GameLogic
{
    public class EntityManager
    {
        private Dictionary<AbsoluteEntityCell, List<SpawnedEntity>> entitiesByAbsoluteCell;
        private Dictionary<string, SpawnedEntity> entitiesByGuid;

        public EntityManager(EntitySpawner entitySpawner)
        {
            entitiesByAbsoluteCell = entitySpawner.GetEntitiesByAbsoluteCell();
            entitiesByGuid = entitiesByAbsoluteCell.Values.SelectMany(l => l).ToDictionary(e => e.Guid, e => e);
        }

        public SpawnedEntity UpdateEntityPosition(string guid, Vector3 position)
        {
            SpawnedEntity entity = GetEntityByGuid(guid);
            AbsoluteEntityCell oldCell = new AbsoluteEntityCell(entity.Position);
            AbsoluteEntityCell newCell = new AbsoluteEntityCell(position);

            if(oldCell != newCell)
            {
                lock (entitiesByAbsoluteCell)
                {
                    List<SpawnedEntity> oldList;

                    if (entitiesByAbsoluteCell.TryGetValue(oldCell, out oldList))
                    {
                        oldList.Remove(entity);
                    }

                    List<SpawnedEntity> newList;

                    if (!entitiesByAbsoluteCell.TryGetValue(newCell, out newList))
                    {
                        newList = new List<SpawnedEntity>();
                        entitiesByAbsoluteCell[newCell] = newList;
                    }

                    newList.Add(entity);
                }
            }

            entity.Position = position;

            return entity;
        }

        public SpawnedEntity GetEntityByGuid(string guid)
        {
            SpawnedEntity entity = null;

            lock (entitiesByGuid)
            {
                entitiesByGuid.TryGetValue(guid, out entity);
            }

            Validate.NotNull(entity);

            return entity;
        }

        public List<SpawnedEntity> GetVisibleEntities(VisibleCell[] cells)
        {
            List<SpawnedEntity> entities = new List<SpawnedEntity>();

            foreach (VisibleCell cell in cells)
            {
                List<SpawnedEntity> cellEntities;

                lock (entitiesByAbsoluteCell)
                {
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
            }

            return entities;
        }

        public void AllowEntitySimulationFor(String playerId, VisibleCell[] added)
        {
            foreach (VisibleCell cell in added)
            {
                List<SpawnedEntity> entities;

                lock (entitiesByAbsoluteCell)
                {
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
        }

        public void RevokeEntitySimulationFor(String playerId, VisibleCell[] removed)
        {
            foreach (VisibleCell cell in removed)
            {
                List<SpawnedEntity> entities;

                lock (entitiesByAbsoluteCell)
                {
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
}

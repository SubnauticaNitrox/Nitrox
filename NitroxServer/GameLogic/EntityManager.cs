using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxServer.GameLogic
{
    public class EntityManager
    {
        private readonly SimulationOwnership simulationOwnership;
        private readonly Dictionary<AbsoluteEntityCell, List<Entity>> entitiesByAbsoluteCell;
        private readonly Dictionary<string, Entity> entitiesByGuid;

        public EntityManager(EntitySpawner entitySpawner, SimulationOwnership simulationOwnership)
        {
            this.simulationOwnership = simulationOwnership;

            entitiesByAbsoluteCell = entitySpawner.GetEntitiesByAbsoluteCell();
            entitiesByGuid = entitiesByAbsoluteCell.Values.SelectMany(l => l).ToDictionary(e => e.Guid, e => e);
        }

        public Entity UpdateEntityPosition(string guid, Vector3 position, Quaternion rotation)
        {
            Entity entity = GetEntityByGuid(guid);
            AbsoluteEntityCell oldCell = new AbsoluteEntityCell(entity.Position, entity.Level);
            AbsoluteEntityCell newCell = new AbsoluteEntityCell(position, entity.Level);

            if (oldCell != newCell)
            {
                lock (entitiesByAbsoluteCell)
                {
                    List<Entity> oldList;

                    if (entitiesByAbsoluteCell.TryGetValue(oldCell, out oldList))
                    {
                        oldList.Remove(entity);
                    }

                    List<Entity> newList;

                    if (!entitiesByAbsoluteCell.TryGetValue(newCell, out newList))
                    {
                        newList = new List<Entity>();
                        entitiesByAbsoluteCell[newCell] = newList;
                    }

                    newList.Add(entity);
                }
            }

            entity.Position = position;
            entity.Rotation = rotation;

            return entity;
        }

        public Entity GetEntityByGuid(string guid)
        {
            Entity entity = null;

            lock (entitiesByGuid)
            {
                entitiesByGuid.TryGetValue(guid, out entity);
            }

            Validate.NotNull(entity);

            return entity;
        }

        public List<Entity> GetVisibleEntities(AbsoluteEntityCell[] cells)
        {
            List<Entity> entities = new List<Entity>();

            foreach (AbsoluteEntityCell cell in cells)
            {
                List<Entity> cellEntities;

                lock (entitiesByAbsoluteCell)
                {
                    if (entitiesByAbsoluteCell.TryGetValue(cell, out cellEntities))
                    {
                        foreach (Entity entity in cellEntities)
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

        public List<Entity> AssignEntitySimulation(Player player, AbsoluteEntityCell[] added)
        {
            List<Entity> assignedEntities = new List<Entity>();

            foreach (AbsoluteEntityCell cell in added)
            {
                List<Entity> entities;

                lock (entitiesByAbsoluteCell)
                {
                    if (entitiesByAbsoluteCell.TryGetValue(cell, out entities))
                    {
                        foreach (Entity entity in entities)
                        {
                            if (cell.Level <= entity.Level && simulationOwnership.TryToAcquire(entity.Guid, player))
                            {
                                assignedEntities.Add(entity);
                            }
                        }
                    }
                }
            }

            return assignedEntities;
        }

        public List<Entity> RevokeEntitySimulationFor(Player player, AbsoluteEntityCell[] removed)
        {
            List<Entity> revokedEntities = new List<Entity>();

            foreach (AbsoluteEntityCell cell in removed)
            {
                List<Entity> entities;

                lock (entitiesByAbsoluteCell)
                {
                    if (entitiesByAbsoluteCell.TryGetValue(cell, out entities))
                    {
                        foreach (Entity entity in entities)
                        {
                            if (entity.Level <= cell.Level && simulationOwnership.RevokeIfOwner(entity.Guid, player))
                            {
                                revokedEntities.Add(entity);
                            }
                        }
                    }
                }
            }

            return revokedEntities;
        }
    }
}

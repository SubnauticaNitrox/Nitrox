using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Logger;

namespace NitroxServer.GameLogic.Entities
{
    public class EntitySimulation
    {
        private readonly EntityData entityData;
        private readonly SimulationOwnership simulationOwnership;
        private readonly PlayerManager playerManager;

        public EntitySimulation(EntityData entityData, SimulationOwnership simulationOwnership, PlayerManager playerManager)
        {
            this.entityData = entityData;
            this.simulationOwnership = simulationOwnership;
            this.playerManager = playerManager;
        }

        public List<OwnedGuid> CalculateSimulationChangesFromCellSwitch(Player player, AbsoluteEntityCell[] added, AbsoluteEntityCell[] removed)
        {
            List<OwnedGuid> ownershipChanges = new List<OwnedGuid>();

            AssignLoadedCellEntitySimulation(player, added, ownershipChanges);

            List<Entity> revokedEntities = RevokeForCells(player, removed);
            AssignEntitiesToNewPlayers(player, revokedEntities, ownershipChanges);

            return ownershipChanges;
        }

        public List<OwnedGuid> CalculateSimulationChangesFromPlayerDisconnect(Player player)
        {
            List<OwnedGuid> ownershipChanges = new List<OwnedGuid>();

            List<Entity> revokedEntities = RevokeAll(player);
            AssignEntitiesToNewPlayers(player, revokedEntities, ownershipChanges);

            return ownershipChanges;
        }

        private void AssignLoadedCellEntitySimulation(Player player, AbsoluteEntityCell[] addedCells, List<OwnedGuid> ownershipChanges)
        {
            List<Entity> entities = AssignForCells(player, addedCells);

            foreach (Entity entity in entities)
            {
                ownershipChanges.Add(new OwnedGuid(entity.Guid, player.Id, true));
            }
        }

        private void AssignEntitiesToNewPlayers(Player oldPlayer, List<Entity> entities, List<OwnedGuid> ownershipChanges)
        {
            foreach (Entity entity in entities)
            {
                AbsoluteEntityCell entityCell = entity.AbsoluteEntityCell;

                foreach (Player player in playerManager.GetPlayers())
                {
                    if (player != oldPlayer && player.HasCellLoaded(entityCell))
                    {
                        Log.Info("Player " + player.Name + " can take over " + entity.Guid);
                        ownershipChanges.Add(new OwnedGuid(entity.Guid, player.Id, true));
                        return;
                    }
                }
            }
        }

        private List<Entity> AssignForCells(Player player, AbsoluteEntityCell[] added)
        {
            List<Entity> assignedEntities = new List<Entity>();

            foreach (AbsoluteEntityCell cell in added)
            {
                List<Entity> entities = entityData.GetEntities(cell);
                
                assignedEntities.AddRange(
                    entities.Where(entity => cell.Level <= entity.Level &&
                                             ((entity.SpawnedByServer && SimulationWhitelist.ForServerSpawned.Contains(entity.TechType)) || !entity.SpawnedByServer) && 
                                             simulationOwnership.TryToAcquire(entity.Guid, player)));                               
            }

            return assignedEntities;
        }

        private List<Entity> RevokeForCells(Player player, AbsoluteEntityCell[] removed)
        {
            List<Entity> revokedEntities = new List<Entity>();

            foreach (AbsoluteEntityCell cell in removed)
            {
                List<Entity> entities = entityData.GetEntities(cell);
                
                revokedEntities.AddRange(
                    entities.Where(entity => entity.Level <= cell.Level && simulationOwnership.RevokeIfOwner(entity.Guid, player)));                        
            }

            return revokedEntities;
        }

        private List<Entity> RevokeAll(Player player)
        {
            List<string> RevokedGuids = simulationOwnership.RevokeAllForOwner(player);

            return entityData.GetEntitiesByGuids(RevokedGuids);
        }
    }
}

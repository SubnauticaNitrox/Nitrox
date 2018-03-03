using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.GameLogic.Entities
{
    public class EntitySimulation
    {
        private readonly EntityData entityData;
        private readonly SimulationOwnership simulationOwnership;

        public EntitySimulation(EntityData entityData, SimulationOwnership simulationOwnership)
        {
            this.entityData = entityData;
            this.simulationOwnership = simulationOwnership;
        }

        public List<Entity> AssignForCells(Player player, AbsoluteEntityCell[] added)
        {
            List<Entity> assignedEntities = new List<Entity>();

            foreach (AbsoluteEntityCell cell in added)
            {
                List<Entity> entities = entityData.GetEntities(cell);
                
                assignedEntities.AddRange(
                    entities.Where(entity => cell.Level <= entity.Level && simulationOwnership.TryToAcquire(entity.Guid, player)));                               
            }

            return assignedEntities;
        }

        public List<Entity> RevokeForCells(Player player, AbsoluteEntityCell[] removed)
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
    }
}

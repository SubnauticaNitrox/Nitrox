using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Bases;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.Packets;
using NitroxModel.Serialization;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.GameLogic.Bases;

public class BuildingManager
{
    private readonly EntityRegistry entityRegistry;
    private readonly WorldEntityManager worldEntityManager;
    private readonly SubnauticaServerConfig config;

    public BuildingManager(EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, SubnauticaServerConfig config)
    {
        this.entityRegistry = entityRegistry;
        this.worldEntityManager = worldEntityManager;
        this.config = config;
    }

    public bool AddGhost(PlaceGhost placeGhost)
    {
        GhostEntity ghostEntity = placeGhost.GhostEntity;
        if (ghostEntity.ParentId == null)
        {
            if (entityRegistry.GetEntityById(ghostEntity.Id).HasValue)
            {
                Log.Error($"Trying to add a ghost to Global Root but another entity with the same id already exists (GhostId: {ghostEntity.Id})");
                return false;
            }

            worldEntityManager.AddOrUpdateGlobalRootEntity(ghostEntity);
            return true;
        }

        if (!entityRegistry.TryGetEntityById(ghostEntity.ParentId, out Entity parentEntity))
        {
            Log.Error($"Trying to add a ghost to a build that isn't registered (ParentId: {ghostEntity.ParentId})");
            return false;
        }
        if (parentEntity is not BuildEntity)
        {
            Log.Error($"Trying to add a ghost to an entity that is not a building (ParentId: {ghostEntity.ParentId})");
            return false;
        }
        if (parentEntity.ChildEntities.Any(childEntity => childEntity.Id.Equals(ghostEntity.Id)))
        {
            Log.Error($"Trying to add a ghost to a building but another child with the same id already exists (GhostId: {ghostEntity.Id})");
            return false;
        }

        worldEntityManager.AddOrUpdateGlobalRootEntity(ghostEntity);
        return true;
    }

    public bool AddModule(PlaceModule placeModule)
    {
        ModuleEntity moduleEntity = placeModule.ModuleEntity;
        if (moduleEntity.ParentId == null)
        {
            if (entityRegistry.GetEntityById(moduleEntity.Id).HasValue)
            {
                Log.Error($"Trying to add a module to Global Root but another entity with the same id already exists ({moduleEntity.Id})");
                return false;
            }

            worldEntityManager.AddOrUpdateGlobalRootEntity(moduleEntity);
            return true;
        }

        if (!entityRegistry.TryGetEntityById(moduleEntity.ParentId, out Entity parentEntity))
        {
            Log.Error($"Trying to add a module to a build that isn't registered (ParentId: {moduleEntity.ParentId})");
            return false;
        }
        if (parentEntity is not BuildEntity && parentEntity is not VehicleWorldEntity)
        {
            Log.Error($"Trying to add a module to an entity that is not a building/vehicle (ParentId: {moduleEntity.ParentId})");
            return false;
        }
        if (parentEntity.ChildEntities.Any(childEntity => childEntity.Id.Equals(moduleEntity.Id)))
        {
            Log.Error($"Trying to add a module to a building but another child with the same id already exists (ModuleId: {moduleEntity.Id})");
            return false;
        }

        worldEntityManager.AddOrUpdateGlobalRootEntity(moduleEntity);
        return true;
    }

    public bool ModifyConstructedAmount(ModifyConstructedAmount modifyConstructedAmount)
    {
        if (!entityRegistry.TryGetEntityById(modifyConstructedAmount.GhostId, out Entity entity))
        {
            Log.Error($"Trying to modify the constructed amount of a non-registered object (GhostId: {modifyConstructedAmount.GhostId})");
            return false;
        }

        // Certain entities with a Constructable are just "regular" WorldEntities (e.g. starship boxes) and for simplicity we'll just not persist their progress
        // since their only use is to be deconstructed to give materials to players
        if (entity is not GhostEntity && entity is not ModuleEntity)
        {
            // In case the entity was fully deconstructed
            if (modifyConstructedAmount.ConstructedAmount == 0f)
            {
                if (entity is GlobalRootEntity)
                {
                    worldEntityManager.RemoveGlobalRootEntity(entity.Id);
                }
                else
                {
                    entityRegistry.RemoveEntity(entity.Id);
                }
            }

            // In any case we'll broadcast the packet
            return true;
        }
        if (modifyConstructedAmount.ConstructedAmount == 0f)
        {
            worldEntityManager.RemoveGlobalRootEntity(entity.Id);
            return true;
        }

        switch (entity)
        {
            case GhostEntity ghostEntity:
                ghostEntity.ConstructedAmount = modifyConstructedAmount.ConstructedAmount;
                break;
            case ModuleEntity moduleEntity:
                moduleEntity.ConstructedAmount = modifyConstructedAmount.ConstructedAmount;
                break;
        }
        return true;
    }

    public bool CreateBase(PlaceBase placeBase)
    {
        if (!entityRegistry.TryGetEntityById(placeBase.FormerGhostId, out Entity entity))
        {
            Log.Error($"Trying to place a base from a non-registered ghost (Id: {placeBase.FormerGhostId})");
            return false;
        }
        if (entity is not GhostEntity)
        {
            Log.Error($"Trying to add a new build to Global Root but another build with the same id already exists (GhostId: {placeBase.FormerGhostId})");
            return false;
        }

        worldEntityManager.RemoveGlobalRootEntity(entity.Id);
        worldEntityManager.AddOrUpdateGlobalRootEntity(placeBase.BuildEntity);
        return true;
    }

    public bool UpdateBase(Player player, UpdateBase updateBase, out int operationId)
    {
        if (!entityRegistry.TryGetEntityById<GhostEntity>(updateBase.FormerGhostId, out _))
        {
            Log.Error($"Trying to place a base from a non-registered ghost (GhostId: {updateBase.FormerGhostId})");
            operationId = -1;
            return false;
        }
        if (!entityRegistry.TryGetEntityById(updateBase.BaseId, out BuildEntity buildEntity))
        {
            Log.Error($"Trying to update a non-registered build (BaseId: {updateBase.BaseId})");
            operationId = -1;
            return false;
        }
        int deltaOperations = buildEntity.OperationId + 1 - updateBase.OperationId;
        if (deltaOperations != 0 && config.SafeBuilding)
        {
            Log.Warn($"Ignoring an {nameof(UpdateBase)} packet from [{player.Name}] which is {Math.Abs(deltaOperations) + (deltaOperations > 0 ? " operations ahead" : " operations late")}");
            NotifyPlayerDesync(player);
            operationId = -1;
            return false;
        }

        worldEntityManager.RemoveGlobalRootEntity(updateBase.FormerGhostId);
        buildEntity.BaseData = updateBase.BaseData;

        foreach (KeyValuePair<NitroxId, NitroxBaseFace> updatedChild in updateBase.UpdatedChildren)
        {
            if (entityRegistry.TryGetEntityById(updatedChild.Key, out InteriorPieceEntity childEntity))
            {
                childEntity.BaseFace = updatedChild.Value;
            }
        }
        foreach (KeyValuePair<NitroxId, NitroxInt3> updatedMoonpool in updateBase.UpdatedMoonpools)
        {
            if (entityRegistry.TryGetEntityById(updatedMoonpool.Key, out MoonpoolEntity childEntity))
            {
                childEntity.Cell = updatedMoonpool.Value;
            }
        }
        foreach (KeyValuePair<NitroxId, NitroxInt3> updatedMapRoom in updateBase.UpdatedMapRooms)
        {
            if (entityRegistry.TryGetEntityById(updatedMapRoom.Key, out MapRoomEntity childEntity))
            {
                childEntity.Cell = updatedMapRoom.Value;
            }
        }

        if (updateBase.BuiltPieceEntity != null && updateBase.BuiltPieceEntity is GlobalRootEntity builtPieceEntity)
        {
            worldEntityManager.AddOrUpdateGlobalRootEntity(builtPieceEntity);
        }

        NitroxId transferFromId = updateBase.ChildrenTransfer.Item1;
        NitroxId transferToId = updateBase.ChildrenTransfer.Item2;
        if (transferFromId != null && transferToId != null)
        {
            // NB: we don't want certain entities to be transferred (e.g. planters)
            entityRegistry.TransferChildren(transferFromId, transferToId, entity => entity is not PlanterEntity);

            // Edge case for when you place a new water park under another one
            if (transferFromId == transferToId && entityRegistry.TryGetEntityById(updateBase.ChildrenTransfer.Item1, out InteriorPieceEntity interiorPieceEntity) &&
                interiorPieceEntity.IsWaterPark)
            {
                // Cleaning all elements from the planter because plants are not kept in this case
                CleanPlanterChildren(interiorPieceEntity);
            }
        }

        // After transferring required children, we need to clean the waterparks that were potentially removed when being merged
        List<NitroxId> removedChildIds = buildEntity.ChildEntities.OfType<InteriorPieceEntity>()
            .Where(entity => entity.IsWaterPark).Select(childEntity => childEntity.Id)
            .Except(updateBase.UpdatedChildren.Keys).ToList();

        foreach (NitroxId removedChildId in removedChildIds)
        {
            if (entityRegistry.GetEntityById(removedChildId).HasValue)
            {
                worldEntityManager.RemoveGlobalRootEntity(removedChildId);
            }
        }
        buildEntity.OperationId++;
        operationId = buildEntity.OperationId;
        return true;
    }

    /// <summary>
    /// Removes all children from the planter(s) of the param water park (<paramref name="interiorPieceEntity"/>)
    /// </summary>
    private void CleanPlanterChildren(InteriorPieceEntity interiorPieceEntity)
    {
        NitroxId leftPlanterId = interiorPieceEntity.Id.Increment();
        NitroxId rightPlanterId = leftPlanterId.Increment();

        // We don't care if either one of those doesn't exist, we just treat both possibility
        // which avoids an o(n) search in interiorPieceEntity's children, and makes it into a o(1) get
        if (entityRegistry.TryGetEntityById(leftPlanterId, out PlanterEntity leftPlanterEntity))
        {
            entityRegistry.CleanChildren(leftPlanterEntity);
        }
        if (entityRegistry.TryGetEntityById(rightPlanterId, out PlanterEntity rightPlanterEntity))
        {
            entityRegistry.CleanChildren(rightPlanterEntity);
        }
    }

    public bool ReplaceBaseByGhost(BaseDeconstructed baseDeconstructed)
    {
        if (!entityRegistry.TryGetEntityById(baseDeconstructed.FormerBaseId, out BuildEntity _))
        {
            Log.Error($"Trying to replace a non-registered build (BaseId: {baseDeconstructed.FormerBaseId})");
            return false;
        }

        worldEntityManager.RemoveGlobalRootEntity(baseDeconstructed.FormerBaseId);
        worldEntityManager.AddOrUpdateGlobalRootEntity(baseDeconstructed.ReplacerGhost);
        return true;
    }

    public bool ReplacePieceByGhost(Player player, PieceDeconstructed pieceDeconstructed, out Entity removedEntity, out int operationId)
    {
        if (!entityRegistry.TryGetEntityById(pieceDeconstructed.BaseId, out BuildEntity buildEntity))
        {
            Log.Error($"Trying to replace a non-registered build (BaseId: {pieceDeconstructed.BaseId})");
            removedEntity = null;
            operationId = -1;
            return false;
        }
        if (entityRegistry.TryGetEntityById(pieceDeconstructed.PieceId, out GhostEntity _))
        {
            Log.Error($"Trying to add a ghost to a building but another ghost child with the same id already exists (GhostId: {pieceDeconstructed.PieceId})");
            removedEntity = null;
            operationId = -1;
            return false;
        }

        int deltaOperations = buildEntity.OperationId + 1 - pieceDeconstructed.OperationId;
        if (deltaOperations != 0 && config.SafeBuilding)
        {
            Log.Warn($"Ignoring a {nameof(PieceDeconstructed)} packet from [{player.Name}] which is {Math.Abs(deltaOperations) + (deltaOperations > 0 ? " operations ahead" : " operations late")}");
            NotifyPlayerDesync(player);
            removedEntity = null;
            operationId = -1;
            return false;
        }

        removedEntity = worldEntityManager.RemoveGlobalRootEntity(pieceDeconstructed.PieceId).Value;
        GhostEntity ghostEntity = pieceDeconstructed.ReplacerGhost;

        worldEntityManager.AddOrUpdateGlobalRootEntity(ghostEntity);
        buildEntity.BaseData = pieceDeconstructed.BaseData;
        buildEntity.OperationId++;
        operationId = buildEntity.OperationId;
        return true;
    }

    public bool CreateWaterParkPiece(WaterParkDeconstructed waterParkDeconstructed, Entity removedEntity)
    {
        if (!entityRegistry.TryGetEntityById(waterParkDeconstructed.BaseId, out BuildEntity buildEntity))
        {
            Log.Error($"Trying to create a WaterPark piece in a non-registered build ({waterParkDeconstructed.BaseId})");
            return false;
        }

        InteriorPieceEntity newPiece = waterParkDeconstructed.NewWaterPark;
        worldEntityManager.AddOrUpdateGlobalRootEntity(newPiece);

        // This part doesn't need all the below register again code because in this very case
        // (a water park split between a below and an above one) the below water park is still the same
        foreach (NitroxId childId in waterParkDeconstructed.MovedChildrenIds)
        {
            entityRegistry.ReparentEntity(childId, newPiece);
        }

        if (removedEntity != null && waterParkDeconstructed.Transfer)
        {
            // Children are all removed by BuildingManager.ReplacePieceByGhost which is called before
            // so we need to re-register them
            // NB: must copy the list before because the below functions do modify newPiece.ChildEntities

            List<Entity> childEntities = [.. removedEntity.ChildEntities.Where(e => e is not PlanterEntity)];

            foreach (Entity childEntity in childEntities)
            {
                entityRegistry.RemoveFromParent(childEntity);
                childEntity.ParentId = newPiece.Id;
                if (childEntity is GlobalRootEntity childGlobalRootEntity)
                {
                    worldEntityManager.AddOrUpdateGlobalRootEntity(childGlobalRootEntity);
                }
                else
                {
                    entityRegistry.AddOrUpdate(childEntity);
                }
            }
        }

        return true;
    }

    public bool SeparateChildrenToWaterParks(LargeWaterParkDeconstructed packet)
    {
        if (packet.MovedChildrenIdsByNewHostId.Count == 0)
        {
            return true;
        }

        if (packet.PieceId == null || !entityRegistry.TryGetEntityById(packet.PieceId, out InteriorPieceEntity deconstructedPiece))
        {
            Log.Error($"Could not find {nameof(InteriorPieceEntity)} with id {packet.PieceId} to be deconstructed");
            return false;
        }

        foreach (KeyValuePair<NitroxId, List<NitroxId>> entry in packet.MovedChildrenIdsByNewHostId)
        {
            if (!entityRegistry.TryGetEntityById(entry.Key, out InteriorPieceEntity waterPark))
            {
                Log.Error($"Could not find {nameof(InteriorPieceEntity)} id {entry.Key} to move {entry.Value} entities to");
                continue;
            }

            foreach (NitroxId entityId in entry.Value)
            {
                entityRegistry.ReparentEntity(entityId, waterPark);
            }
        }

        return true;
    }

    private void NotifyPlayerDesync(Player player)
    {
        Dictionary<NitroxId, int> operations = GetEntitiesOperations(worldEntityManager.GetGlobalRootEntities(true));
        player.SendPacket(new BuildingDesyncWarning(operations));
    }

    public static Dictionary<NitroxId, int> GetEntitiesOperations(List<GlobalRootEntity> entities)
    {
        return entities.OfType<BuildEntity>().ToDictionary(entity => entity.Id, entity => entity.OperationId);
    }
}

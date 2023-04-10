using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.GameLogic.Bases;

public class BuildingManager
{
    private readonly EntityRegistry entityRegistry;
    private readonly WorldEntityManager worldEntityManager;

    public BuildingManager(EntityRegistry entityRegistry, WorldEntityManager worldEntityManager)
    {
        // TODO: Make a queue to treat requests one by one depending on the ParentId
        this.entityRegistry = entityRegistry;
        this.worldEntityManager = worldEntityManager;
    }

    // TODO: When one of these functions return false, we should notify the client that he is desynced and resync him accordingly

    public bool AddGhost(PlaceGhost placeGhost)
    {
        GhostEntity ghostEntity = placeGhost.GhostEntity;
        if (ghostEntity.ParentId != null)
        {
            if (entityRegistry.TryGetEntityById(ghostEntity.ParentId, out Entity parentEntity))
            {
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
                parentEntity.ChildEntities.Add(ghostEntity);
                worldEntityManager.AddGlobalRootEntity(ghostEntity);
                Log.Debug($"Added a GhostEntity: {ghostEntity.Id}, under {parentEntity.Id}");
                return true;
            }
            Log.Error($"Trying to add a ghost to a build that isn't registered (ParentId: {ghostEntity.ParentId})");
            return false;
        }

        if (entityRegistry.GetEntityById(ghostEntity.Id).HasValue)
        {
            Log.Error($"Trying to add a ghost to Global Root but another entity with the same id already exists (GhostId: {ghostEntity.Id})");
            return false;
        }

        worldEntityManager.AddGlobalRootEntity(ghostEntity);
        Log.Debug($"Added a GhostEntity: {ghostEntity.Id}");
        return true;
    }

    public bool AddModule(PlaceModule placeModule)
    {
        ModuleEntity moduleEntity = placeModule.ModuleEntity;
        if (moduleEntity.ParentId != null)
        {
            if (entityRegistry.TryGetEntityById(moduleEntity.ParentId, out Entity parentEntity))
            {
                if (parentEntity is not BuildEntity)
                {
                    Log.Error($"Trying to add a ghost to an entity that is not a building (ParentId: {moduleEntity.ParentId})");
                    return false;
                }
                if (parentEntity.ChildEntities.Any(childEntity => childEntity.Id.Equals(moduleEntity.Id)))
                {
                    Log.Error($"Trying to add a module to a building but another child with the same id already exists (ModuleId: {moduleEntity.Id})");
                    return false;
                }
                parentEntity.ChildEntities.Add(moduleEntity);
                worldEntityManager.AddGlobalRootEntity(moduleEntity);
                return true;
            }
            Log.Error($"Trying to add a module to a build that isn't registered (ParentId: {moduleEntity.ParentId})");
            return false;
        }

        if (entityRegistry.GetEntityById(moduleEntity.Id).HasValue)
        {
            Log.Error($"Trying to add a module to Global Root but another entity with the same id already exists ({moduleEntity.Id})");
            return false;
        }

        worldEntityManager.AddGlobalRootEntity(moduleEntity);
        return true;
    }

    public bool ModifyConstructedAmount(ModifyConstructedAmount modifyConstructedAmount)
    {
        if (entityRegistry.TryGetEntityById(modifyConstructedAmount.GhostId, out Entity entity))
        {
            if (entity is not GhostEntity && entity is not ModuleEntity)
            {
                Log.Error($"Trying to modify the constructed amount of an entity that is not a ghost (Id: {modifyConstructedAmount.GhostId})");
                return false;
            }
            if (modifyConstructedAmount.ConstructedAmount == 0f)
            {
                worldEntityManager.RemoveGlobalRootEntity(entity.Id);
                return true;
            }
            if (entity is GhostEntity ghostEntity)
            {
                ghostEntity.ConstructedAmount = modifyConstructedAmount.ConstructedAmount;
            }
            else if (entity is ModuleEntity moduleEntity)
            {
                moduleEntity.ConstructedAmount = modifyConstructedAmount.ConstructedAmount;
            }
            return true;
        }
        Log.Error($"Trying to modify the constructed amount of a non-registered object (GhostId: {modifyConstructedAmount.GhostId})");
        return false;
    }

    public bool CreateBase(PlaceBase placeBase)
    {
        if (entityRegistry.TryGetEntityById(placeBase.FormerGhostId, out Entity entity))
        {
            if (entity is not GhostEntity)
            {
                Log.Error($"Trying to add a new build to Global Root but another build with the same id already exists (GhostId: {placeBase.FormerGhostId})");
                return false;
            }
            worldEntityManager.RemoveGlobalRootEntity(entity.Id);

            worldEntityManager.AddGlobalRootEntity(placeBase.BuildEntity);
            return true;
        }
        Log.Error($"Trying to place a base from a non-registered ghost (Id: {placeBase.FormerGhostId})");
        return false;
    }

    public bool UpdateBase(UpdateBase updateBase)
    {
        if (!entityRegistry.TryGetEntityById(updateBase.FormerGhostId, out Entity entity) || entity is not GhostEntity)
        {
            Log.Error($"Tring to place a base from a non-registered ghost (GhostId: {updateBase.FormerGhostId})");
            return false;
        }
        if (!entityRegistry.TryGetEntityById(updateBase.BaseId, out entity) || entity is not BuildEntity buildEntity)
        {
            Log.Error($"Trying to update a non-registered build (BaseId: {updateBase.BaseId})");
            return false;
        }
        worldEntityManager.RemoveGlobalRootEntity(updateBase.FormerGhostId);
        buildEntity.SavedBase = updateBase.SavedBase;

        // We need to clean the waterparks that were potentially removed when merging
        List<NitroxId> removedChildIds = buildEntity.ChildEntities.Select(childEntity => childEntity.Id)
            .Except(updateBase.ChildEntities.Select(childEntity => childEntity.Id)).ToList();
        foreach (NitroxId removedChildId in removedChildIds)
        {
            if (entityRegistry.TryGetEntityById(removedChildId, out Entity removedEntity) && removedEntity is InteriorPieceEntity)
            {
                worldEntityManager.RemoveGlobalRootEntity(removedChildId);
            }
        }

        foreach (Entity childEntity in updateBase.ChildEntities)
        {
            if (childEntity is GlobalRootEntity globalRootChildEntity)
            {
                worldEntityManager.UpdateGlobalRootEntity(globalRootChildEntity);
                continue;
            }
            entityRegistry.AddOrUpdate(childEntity);
        }
        return true;
    }

    public bool ReplaceBaseByGhost(BaseDeconstructed baseDeconstructed)
    {
        if (!entityRegistry.TryGetEntityById(baseDeconstructed.FormerBaseId, out Entity entity) || entity is not BuildEntity)
        {
            Log.Error($"Trying to replace a non-registered build (BaseId: {baseDeconstructed.FormerBaseId})");
            return false;
        }

        worldEntityManager.RemoveGlobalRootEntity(baseDeconstructed.FormerBaseId);
        worldEntityManager.AddGlobalRootEntity(baseDeconstructed.ReplacerGhost);
        return true;
    }

    public bool ReplacePieceByGhost(PieceDeconstructed pieceDeconstructed)
    {
        if (!entityRegistry.TryGetEntityById(pieceDeconstructed.BaseId, out Entity entity) || entity is not BuildEntity buildEntity)
        {
            Log.Error($"Trying to replace a non-registered build (BaseId: {pieceDeconstructed.BaseId})");
            return false;
        }
        if (entity.ChildEntities.Any(childEntity => childEntity.Id.Equals(pieceDeconstructed.PieceId) && childEntity is GhostEntity))
        {
            Log.Error($"Trying to add a ghost to a building but another ghost child with the same id already exists (GhostId: {pieceDeconstructed.PieceId})");
            return false;
        }
        worldEntityManager.RemoveGlobalRootEntity(pieceDeconstructed.PieceId);
        GhostEntity ghostEntity = pieceDeconstructed.ReplacerGhost;
        
        buildEntity.ChildEntities.Add(ghostEntity);
        buildEntity.SavedBase = pieceDeconstructed.SavedBase;
        worldEntityManager.AddGlobalRootEntity(ghostEntity);
        return true;
    }

    public bool CreateWaterParkPiece(WaterParkDeconstructed waterParkDeconstructed)
    {
        if (!entityRegistry.TryGetEntityById(waterParkDeconstructed.BaseId, out Entity entity) || entity is not BuildEntity buildEntity)
        {
            Log.Error($"Trying to create a WaterPark piece in a non-registered build ({waterParkDeconstructed.BaseId})");
            return false;
        }
        if (buildEntity.ChildEntities.Any(childEntity => childEntity.Id.Equals(waterParkDeconstructed.NewWaterPark.Id)))
        {
            Log.Error($"Trying to create a WaterPark piece with an already registered id ({waterParkDeconstructed.NewWaterPark.Id})");
            return false;
        }
        worldEntityManager.AddGlobalRootEntity(waterParkDeconstructed.NewWaterPark);

        buildEntity.ChildEntities.Add(waterParkDeconstructed.NewWaterPark);
        return true;
    }
}

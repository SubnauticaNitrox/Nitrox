using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.New;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.GameLogic.Bases;

public class BuildingManager
{
    public SavedGlobalRoot GlobalRoot
    {
        get
        {
            return new()
            {
                Builds = Builds.Values.ToList(),
                Modules = Modules.Values.ToList(),
                Ghosts = Ghosts.Values.ToList()
            };
        }
    }

    internal readonly ThreadSafeDictionary<NitroxId, SavedBuild> Builds = new();
    internal readonly ThreadSafeDictionary<NitroxId, SavedModule> Modules = new();
    internal readonly ThreadSafeDictionary<NitroxId, SavedGhost> Ghosts = new();

    private readonly ThreadSafeDictionary<NitroxId, NitroxGhost> allGhosts = new();
    private readonly ThreadSafeDictionary<NitroxId, NitroxModule> allModules = new();

    private readonly EntityRegistry entityRegistry;
    private readonly WorldEntityManager worldEntityManager;

    public BuildingManager(SavedGlobalRoot previousData, EntityRegistry entityRegistry, WorldEntityManager worldEntityManager)
    {
        // TODO: Make a queue to treat requests one by one depending on the ParentId
        previousData ??= new();
        this.entityRegistry = entityRegistry;
        this.worldEntityManager = worldEntityManager;
        return;

        foreach (SavedBuild savedBuild in previousData.Builds)
        {
            Builds.Add(savedBuild.NitroxId, savedBuild);
            foreach (SavedGhost savedGhost in savedBuild.Ghosts)
            {
                allGhosts.Add(savedGhost.NitroxId, new(savedBuild, savedGhost));
            }
            foreach (SavedModule savedModule in savedBuild.Modules)
            {
                allModules.Add(savedModule.NitroxId, new(savedBuild, savedModule));
            }
        }
        foreach (SavedModule savedModule in previousData.Modules)
        {
            Modules.Add(savedModule.NitroxId, savedModule);
            allModules.Add(savedModule.NitroxId, new(null, savedModule));
        }
        foreach (SavedGhost savedGhost in previousData.Ghosts)
        {
            Ghosts.Add(savedGhost.NitroxId, savedGhost);
            allGhosts.Add(savedGhost.NitroxId, new(null, savedGhost));
        }
    }

    // TODO: When one of these functions return false, we should notify the client that he is desynced and resync him accordingly

    public bool AddGhost(PlaceGhost placeGhost)
    {
        if (placeGhost.ParentId != null)
        {
            if (entityRegistry.TryGetEntityById(placeGhost.ParentId, out Entity parentEntity))
            {
                if (parentEntity is not BuildEntity)
                {
                    Log.Error($"Trying to add a ghost to an entity that is not a building (ParentId: {placeGhost.ParentId})");
                    return false;
                }
                if (parentEntity.ChildEntities.Any(childEntity => childEntity.Id.Equals(placeGhost.SavedGhost.NitroxId)))
                {
                    Log.Error($"Trying to add a ghost to a building but another child with the same id already exists (GhostId: {placeGhost.SavedGhost.NitroxId})");
                    return false;
                }
                GhostEntity ghostChildEntity = new(placeGhost.SavedGhost, placeGhost.ParentId);
                parentEntity.ChildEntities.Add(ghostChildEntity);
                entityRegistry.AddEntity(ghostChildEntity);
                worldEntityManager.AddGlobalRootEntity(ghostChildEntity);
                Log.Debug($"Added a GhostEntity: {ghostChildEntity.Id}");
                return true;
            }
            Log.Error($"Trying to add a ghost to a build that isn't registered (ParentId: {placeGhost.ParentId})");
            return false;
        }

        if (entityRegistry.GetEntityById(placeGhost.SavedGhost.NitroxId).HasValue)
        {
            Log.Error($"Trying to add a ghost to Global Root but another entity with the same id already exists (GhostId: {placeGhost.SavedGhost.NitroxId})");
            return false;
        }

        GhostEntity ghostEntity = new(placeGhost.SavedGhost);
        entityRegistry.AddEntity(ghostEntity);
        worldEntityManager.AddGlobalRootEntity(ghostEntity);
        Log.Debug($"Added a GhostEntity: {ghostEntity.Id}");
        return true;
    }

    public bool AddModule(PlaceModule placeModule)
    {
        if (placeModule.ParentId != null)
        {
            if (entityRegistry.TryGetEntityById(placeModule.ParentId, out Entity parentEntity))
            {
                if (parentEntity is not BuildEntity)
                {
                    Log.Error($"Trying to add a ghost to an entity that is not a building (ParentId: {placeModule.ParentId})");
                    return false;
                }
                if (parentEntity.ChildEntities.Any(childEntity => childEntity.Id.Equals(placeModule.SavedModule.NitroxId)))
                {
                    Log.Error($"Trying to add a module to a building but another child with the same id already exists (ModuleId: {placeModule.SavedModule.NitroxId})");
                    return false;
                }
                ModuleEntity moduleChildEntity = new(placeModule.SavedModule, placeModule.ParentId);
                parentEntity.ChildEntities.Add(moduleChildEntity);
                entityRegistry.AddEntity(moduleChildEntity);
                worldEntityManager.AddGlobalRootEntity(moduleChildEntity);
                return true;
            }
            Log.Error($"Trying to add a module to a build that isn't registered (ParentId: {placeModule.ParentId})");
            return false;
        }

        if (entityRegistry.GetEntityById(placeModule.SavedModule.NitroxId).HasValue)
        {
            Log.Error($"Trying to add a module to Global Root but another entity with the same id already exists ({placeModule.SavedModule.NitroxId})");
            return false;
        }

        ModuleEntity moduleEntity = new(placeModule.SavedModule);
        entityRegistry.AddEntity(moduleEntity);
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
                entityRegistry.RemoveEntity(entity.Id);
                worldEntityManager.RemoveGlobalRootEntity(entity.Id);
                return true;
            }
            if (entity is GhostEntity ghostEntity)
            {
                ghostEntity.SavedGhost.ConstructedAmount = modifyConstructedAmount.ConstructedAmount;
            }
            else if (entity is ModuleEntity moduleEntity)
            {
                moduleEntity.SavedModule.ConstructedAmount = modifyConstructedAmount.ConstructedAmount;
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
            entityRegistry.RemoveEntity(entity.Id);
            worldEntityManager.RemoveGlobalRootEntity(entity.Id);

            BuildEntity buildEntity = new(placeBase.SavedBuild);
            entityRegistry.AddEntity(buildEntity);
            worldEntityManager.AddGlobalRootEntity(buildEntity);
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
        entityRegistry.RemoveEntity(updateBase.FormerGhostId);
        worldEntityManager.RemoveGlobalRootEntity(updateBase.FormerGhostId);
        buildEntity.SavedBuild = updateBase.SavedBuild;
        return true;
    }

    public bool ReplaceBaseByGhost(BaseDeconstructed baseDeconstructed)
    {
        if (!entityRegistry.TryGetEntityById(baseDeconstructed.FormerBaseId, out Entity entity) || entity is not BuildEntity)
        {
            Log.Error($"Trying to replace a non-registered build (BaseId: {baseDeconstructed.FormerBaseId})");
            return false;
        }
        // TODO: Reparent the base's players children to not delete them.
        entityRegistry.RemoveEntity(baseDeconstructed.FormerBaseId);
        worldEntityManager.RemoveGlobalRootEntity(baseDeconstructed.FormerBaseId);

        GhostEntity ghostEntity = new(baseDeconstructed.ReplacerGhost);
        entityRegistry.AddEntity(ghostEntity);
        worldEntityManager.AddGlobalRootEntity(ghostEntity);
        return true;
    }

    public bool ReplacePieceByGhost(PieceDeconstructed pieceDeconstructed)
    {
        if (!entityRegistry.TryGetEntityById(pieceDeconstructed.BaseId, out Entity entity) || entity is not BuildEntity buildEntity)
        {
            Log.Error($"Trying to replace a non-registered build (BaseId: {pieceDeconstructed.BaseId})");
            return false;
        }
        if (buildEntity.ChildEntities.Any(childEntity => childEntity.Id.Equals(pieceDeconstructed.PieceId) && childEntity is GhostEntity))
        {
            Log.Error($"Trying to add a ghost to a building but another ghost child with the same id already exists (GhostId: {pieceDeconstructed.PieceId})");
            return false;
        }
        entityRegistry.RemoveEntity(pieceDeconstructed.PieceId);
        worldEntityManager.RemoveGlobalRootEntity(pieceDeconstructed.PieceId);

        GhostEntity ghostChildEntity = new(pieceDeconstructed.ReplacerGhost, pieceDeconstructed.BaseId);
        entityRegistry.AddEntity(ghostChildEntity);
        worldEntityManager.AddGlobalRootEntity(ghostChildEntity);

        buildEntity.ChildEntities.Add(ghostChildEntity);
        buildEntity.SavedBuild.Base = pieceDeconstructed.SavedBase;
        return true;
    }

    public bool CreateWaterParkPiece(WaterParkDeconstructed waterParkDeconstructed)
    {
        if (!entityRegistry.TryGetEntityById(waterParkDeconstructed.BaseId, out Entity entity) || entity is not BuildEntity buildEntity)
        {
            Log.Error($"Trying to create a WaterPark piece in a non-registered build ({waterParkDeconstructed.BaseId})");
            return false;
        }
        if (buildEntity.ChildEntities.Any(childEntity => childEntity.Id.Equals(waterParkDeconstructed.NewWaterPark.NitroxId)))
        {
            Log.Error($"Trying to create a WaterPark piece with an already registered id ({waterParkDeconstructed.NewWaterPark.NitroxId})");
            return false;
        }
        InteriorPieceEntity waterParkEntity = new(waterParkDeconstructed.NewWaterPark, buildEntity.Id);
        entityRegistry.AddEntity(waterParkEntity);
        worldEntityManager.AddGlobalRootEntity(waterParkEntity);

        buildEntity.ChildEntities.Add(waterParkEntity);
        return true;
    }

    class NitroxGhost
    {
        public SavedBuild Parent;
        public SavedGhost SavedGhost;

        public NitroxGhost(SavedBuild parent, SavedGhost savedGhost)
        {
            Parent = parent;
            SavedGhost = savedGhost;
        }
    }

    class NitroxModule
    {
        public SavedBuild Parent;
        public SavedModule SavedModule;

        public NitroxModule(SavedBuild parent, SavedModule savedModule)
        {
            Parent = parent;
            SavedModule = savedModule;
        }
    }
}

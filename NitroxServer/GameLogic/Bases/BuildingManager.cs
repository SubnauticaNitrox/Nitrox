using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.New;
using NitroxModel.Packets;

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

    public BuildingManager(SavedGlobalRoot previousData)
    {
        previousData ??= new();

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
            lock (Builds)
            {
                if (Builds.TryGetValue(placeGhost.ParentId, out SavedBuild savedBuild))
                {
                    if (savedBuild.Ghosts.Any(ghost => ghost.NitroxId.Equals(placeGhost.SavedGhost.NitroxId)))
                    {
                        Log.Error($"Trying to add a ghost to a building but another ghost with the same id already exists ({placeGhost.SavedGhost.NitroxId})");
                        return false;
                    }
                    Log.Debug("A");
                    savedBuild.Ghosts.Add(placeGhost.SavedGhost);
                    allGhosts.Add(placeGhost.SavedGhost.NitroxId, new(savedBuild, placeGhost.SavedGhost));
                    return true;
                }
                else
                {
                    Log.Error($"Trying to add a ghost to a build that isn't registered ({placeGhost.ParentId})");
                    return false;
                }
            }
        }

        lock (Ghosts)
        {
            if (Ghosts.ContainsKey(placeGhost.SavedGhost.NitroxId))
            {
                Log.Error($"Trying to add a ghost to Global Root but another ghost with the same id already exists ({placeGhost.SavedGhost.NitroxId})");
                return false;
            }
            Log.Debug("B");
            Ghosts.Add(placeGhost.SavedGhost.NitroxId, placeGhost.SavedGhost);
            allGhosts.Add(placeGhost.SavedGhost.NitroxId, new(null, placeGhost.SavedGhost));
            return true;
        }
    }

    public bool AddModule(PlaceModule placeModule)
    {
        if (placeModule.ParentId != null)
        {
            lock (Builds)
            {
                if (Builds.TryGetValue(placeModule.ParentId, out SavedBuild savedBuild))
                {
                    if (savedBuild.Modules.Any(module => module.NitroxId.Equals(placeModule.SavedModule.NitroxId)))
                    {
                        Log.Error($"Trying to add a module to a building but another module with the same id already exists ({placeModule.SavedModule.NitroxId})");
                        return false;
                    }
                    savedBuild.Modules.Add(placeModule.SavedModule);
                    allModules.Add(placeModule.SavedModule.NitroxId, new(savedBuild, placeModule.SavedModule));
                    return true;
                }
                else
                {
                    Log.Error($"Trying to add a module to a build that isn't registered ({placeModule.ParentId})");
                    return false;
                }
            }
        }

        lock (Modules)
        {
            if (Modules.ContainsKey(placeModule.SavedModule.NitroxId))
            {
                Log.Error($"Trying to add a module to Global Root but another module with the same id already exists ({placeModule.SavedModule.NitroxId})");
                return false;
            }
            Modules.Add(placeModule.SavedModule.NitroxId, placeModule.SavedModule);
            allModules.Add(placeModule.SavedModule.NitroxId, new(null, placeModule.SavedModule));
            return true;
        }
    }

    public bool ModifyConstructedAmount(ModifyConstructedAmount modifyConstructedAmount)
    {
        lock (Builds)
        {
            lock (Ghosts)
            {
                if (allGhosts.TryGetValue(modifyConstructedAmount.GhostId, out NitroxGhost nitroxGhost))
                {
                    if (modifyConstructedAmount.ConstructedAmount == 0f)
                    {
                        if (nitroxGhost.Parent != null)
                        {
                            nitroxGhost.Parent.Ghosts.Remove(nitroxGhost.SavedGhost);
                        }
                        else
                        {
                            Ghosts.Remove(modifyConstructedAmount.GhostId);
                        }
                        allGhosts.Remove(modifyConstructedAmount.GhostId);
                        return true;
                    }
                    nitroxGhost.SavedGhost.ConstructedAmount = modifyConstructedAmount.ConstructedAmount;
                    return true;
                }
            }
            lock (Modules)
            {
                if (!allModules.TryGetValue(modifyConstructedAmount.GhostId, out NitroxModule nitroxModule))
                {
                    Log.Error($"Trying to modify the constructed amount of a non-registered object ({modifyConstructedAmount.GhostId})");
                    return false;
                }
                if (modifyConstructedAmount.ConstructedAmount == 0f)
                {
                    if (nitroxModule.Parent != null)
                    {
                        nitroxModule.Parent.Modules.Remove(nitroxModule.SavedModule);
                    }
                    else
                    {
                        Modules.Remove(modifyConstructedAmount.GhostId);
                    }
                    allModules.Remove(modifyConstructedAmount.GhostId);
                    return true;
                }
                nitroxModule.SavedModule.ConstructedAmount = modifyConstructedAmount.ConstructedAmount;
                return true;
            }
        }
    }

    public bool AddBase(PlaceBase placeBase)
    {
        lock (Ghosts)
        {
            lock (Builds)
            {
                if (!Ghosts.ContainsKey(placeBase.FormerGhostId))
                {
                    Log.Error($"Trying to place a base from a non-registered ghost ({placeBase.FormerGhostId})");
                    return false;
                }
                if (Builds.ContainsKey(placeBase.FormerGhostId))
                {
                    Log.Error($"Trying to add a new build to Global Root but another build with the same id already exists ({placeBase.FormerGhostId})");
                    return false;
                }
                Ghosts.Remove(placeBase.FormerGhostId);
                allGhosts.Remove(placeBase.FormerGhostId);
                Builds.Add(placeBase.SavedBuild.NitroxId, placeBase.SavedBuild);
                return true;
            }
        }
    }

    public bool UpdateBase(UpdateBase updateBase)
    {
        lock (Ghosts)
        {
            lock (Builds)
            {
                if (!allGhosts.ContainsKey(updateBase.FormerGhostId))
                {
                    Log.Error($"Tring to place a base from a non-registered ghost ({updateBase.FormerGhostId})");
                    return false;
                }
                if (!Builds.ContainsKey(updateBase.BaseId))
                {
                    Log.Error($"Trying to update a non-registered build ({updateBase.BaseId})");
                    return false;
                }

                Ghosts.Remove(updateBase.FormerGhostId);
                allGhosts.Remove(updateBase.FormerGhostId);
                Builds[updateBase.BaseId] = updateBase.SavedBuild;
                return true;
            }
        }
    }

    public bool ReplaceBaseByGhost(BaseDeconstructed baseDeconstructed)
    {
        lock (Builds)
        {
            lock (Ghosts)
            {
                if (!Builds.ContainsKey(baseDeconstructed.FormerBaseId))
                {
                    Log.Error($"Trying to replace a non-registered build ({baseDeconstructed.FormerBaseId})");
                    return false;
                }
                if (Ghosts.ContainsKey(baseDeconstructed.ReplacerGhost.NitroxId))
                {
                    Log.Error($"Trying to add a ghost to Global Root but another ghost with the same id already exists ({baseDeconstructed.ReplacerGhost.NitroxId})");
                    return false;
                }
                Builds.Remove(baseDeconstructed.FormerBaseId);
                Ghosts.Add(baseDeconstructed.ReplacerGhost.NitroxId, baseDeconstructed.ReplacerGhost);
                allGhosts.Add(baseDeconstructed.ReplacerGhost.NitroxId, new(null, baseDeconstructed.ReplacerGhost));
                return true;
            }
        }
    }

    public bool ReplacePieceByGhost(PieceDeconstructed pieceDeconstructed)
    {
        lock (Builds)
        {
            lock (Ghosts)
            {
                if (!Builds.TryGetValue(pieceDeconstructed.BaseId, out SavedBuild parentBuild))
                {
                    Log.Error($"Trying to replace a piece in a non-registered build ({pieceDeconstructed.BaseId})");
                    return false;
                }
                if (parentBuild.Ghosts.Any(ghost => ghost.NitroxId.Equals(pieceDeconstructed.PieceId)))
                {
                    Log.Error($"Trying to add a ghost to a build but another ghost with the same id already exists ({pieceDeconstructed.PieceId})");
                    return false;
                }
                
                int interiorPieceIndex = parentBuild.InteriorPieces.FindIndex(interiorPiece => interiorPiece.NitroxId.Equals(pieceDeconstructed.PieceId));
                if (interiorPieceIndex != -1)
                {
                    parentBuild.InteriorPieces.RemoveAt(interiorPieceIndex);
                }
                parentBuild.Ghosts.Add(pieceDeconstructed.ReplacerGhost);
                allGhosts.Add(pieceDeconstructed.PieceId, new(parentBuild, pieceDeconstructed.ReplacerGhost));
                parentBuild.Base = pieceDeconstructed.SavedBase;
                return true;
            }
        }
    }

    public bool CreateWaterParkPiece(WaterParkDeconstructed waterParkDeconstructed)
    {
        lock (Builds)
        {
            if (!Builds.TryGetValue(waterParkDeconstructed.BaseId, out SavedBuild parentBuild))
            {
                Log.Error($"Trying to create a WaterPark piece in a non-registered build ({waterParkDeconstructed.BaseId})");
                return false;
            }
            if (parentBuild.InteriorPieces.Any(piece => piece.NitroxId.Equals(waterParkDeconstructed.NewWaterPark.NitroxId)))
            {
                Log.Error($"Trying to create a WaterPark piece with an already registered id ({waterParkDeconstructed.NewWaterPark.NitroxId})");
                return false;
            }
            parentBuild.InteriorPieces.Add(waterParkDeconstructed.NewWaterPark);
            return true;
        }
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

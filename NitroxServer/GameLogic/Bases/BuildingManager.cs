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

    public BuildingManager(SavedGlobalRoot previousGlobalRoot)
    {
        previousGlobalRoot ??= new() { Builds = new(), Modules = new(), Ghosts = new() };
        foreach (SavedBuild savedBuild in previousGlobalRoot.Builds)
        {
            Builds.Add(savedBuild.NitroxId, savedBuild);
        }
        foreach (SavedModule savedModule in previousGlobalRoot.Modules)
        {
            Modules.Add(savedModule.NitroxId, savedModule);
        }
        foreach (SavedGhost savedGhost in previousGlobalRoot.Ghosts)
        {
            Ghosts.Add(savedGhost.NitroxId, savedGhost);
        }
    }

    // TODO: When one of these functions return false, we should notify the client that he is desynced and resync him accordingly

    public bool AddGhost(PlaceGhost placeGhost)
    {
        lock (GlobalRoot)
        {
            if (placeGhost.ParentId != null)
            {
                if (Builds.TryGetValue(placeGhost.ParentId, out SavedBuild savedBuild))
                {
                    if (savedBuild.Ghosts.Any(ghost => ghost.NitroxId.Equals(placeGhost.SavedGhost.NitroxId)))
                    {
                        Log.Error($"Trying to add a ghost to a building but another ghost with the same id already exists ({placeGhost.SavedGhost.NitroxId})");
                        return false;
                    }
                    savedBuild.Ghosts.Add(placeGhost.SavedGhost);
                    return true;
                }
                else
                {
                    Log.Error($"Trying to add a ghost to a build that isn't registered ({placeGhost.ParentId})");
                    return false;
                }
            }

            if (Ghosts.ContainsKey(placeGhost.SavedGhost.NitroxId))
            {
                Log.Error($"Trying to add a ghost to Global Root but another ghost with the same id already exists ({placeGhost.SavedGhost.NitroxId})");
                return false;
            }
            Ghosts.Add(placeGhost.SavedGhost.NitroxId, placeGhost.SavedGhost);
            return true;
        }
    }

    public bool AddModule(PlaceModule placeModule)
    {
        return true;
        /*lock (GlobalRoot)
        {
            if (placeGhost.ParentId != null)
            {
                if (Builds.TryGetValue(placeGhost.ParentId, out SavedBuild savedBuild))
                {
                    if (savedBuild.Ghosts.Any(ghost => ghost.NitroxId.Equals(placeGhost.SavedGhost.NitroxId)))
                    {
                        Log.Error($"Trying to add a ghost to a building but another ghost with the same id already exists ({placeGhost.SavedGhost.NitroxId})");
                        return false;
                    }
                    savedBuild.Ghosts.Add(placeGhost.SavedGhost);
                    return true;
                }
                else
                {
                    Log.Error($"Trying to add a ghost to a build that isn't registered ({placeGhost.ParentId})");
                    return false;
                }
            }

            if (Ghosts.ContainsKey(placeGhost.SavedGhost.NitroxId))
            {
                Log.Error($"Trying to add a ghost to Global Root but another ghost with the same id already exists ({placeGhost.SavedGhost.NitroxId})");
                return false;
            }
            Ghosts.Add(placeGhost.SavedGhost.NitroxId, placeGhost.SavedGhost);
            return true;
        }*/
    }
}

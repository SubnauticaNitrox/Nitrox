using NitroxModel.DataStructures.GameLogic.Bases;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.EntityUtils;

public static class NitroxGlobalRoot
{
    public static SavedGlobalRoot From(Transform globalRoot)
    {
        SavedGlobalRoot savedGlobalRoot = new() { Builds = new(), Modules = new(), Ghosts = new() };
        foreach (Transform child in globalRoot)
        {
            if (child.TryGetComponent(out Base @base))
            {
                savedGlobalRoot.Builds.Add(NitroxBuild.From(@base));
            }
            else if (child.TryGetComponent(out Constructable constructable))
            {
                if (constructable is ConstructableBase constructableBase)
                {
                    savedGlobalRoot.Ghosts.Add(NitroxGhost.From(constructableBase));
                    continue;
                }
                savedGlobalRoot.Modules.Add(NitroxModule.From(constructable));
            }
        }
        return savedGlobalRoot;
    }

    public static string ToString(this SavedGlobalRoot savedGlobalRoot)
    {
        return $"SavedGlobalRoot [Builds: {savedGlobalRoot.Builds.Count}, Modules: {savedGlobalRoot.Modules.Count}, Ghosts: {savedGlobalRoot.Ghosts.Count}]";
    }
}

using NitroxModel.DataStructures.GameLogic.Entities;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.EntityUtils;

public static class NitroxGlobalRoot
{
    public static List<GlobalRootEntity> From(Transform globalRoot)
    {
        List<GlobalRootEntity> entities = new();
        foreach (Transform child in globalRoot)
        {
            if (child.TryGetComponent(out Base @base))
            {
                entities.Add(NitroxBuild.From(@base));
            }
            else if (child.TryGetComponent(out Constructable constructable))
            {
                if (constructable is ConstructableBase constructableBase)
                {
                    entities.Add(NitroxGhost.From(constructableBase));
                    continue;
                }
                entities.Add(NitroxModule.From(constructable));
            }
        }
        return entities;
    }
}

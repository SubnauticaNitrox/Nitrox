using System.Collections.Generic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities;

public class GlobalRootData
{
    public List<GlobalRootEntity> Entities = [];

    public static GlobalRootData From(List<GlobalRootEntity> entities)
    {
        return new GlobalRootData { Entities = entities };
    }
}

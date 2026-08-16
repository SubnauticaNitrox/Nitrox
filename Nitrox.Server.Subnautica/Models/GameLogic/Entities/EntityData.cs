using System.Collections.Generic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities;

public class EntityData
{
    public List<Entity> Entities = [];

    public static EntityData From(List<Entity> entities)
    {
        return new EntityData { Entities = entities };
    }
}

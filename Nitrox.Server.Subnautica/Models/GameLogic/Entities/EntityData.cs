using System.Collections.Generic;
using System.Runtime.Serialization;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities;

[DataContract]
public class EntityData
{
    [DataMember(Order = 1)]
    public List<Entity> Entities = [];

    public static EntityData From(List<Entity> entities)
    {
        return new EntityData { Entities = entities };
    }
}

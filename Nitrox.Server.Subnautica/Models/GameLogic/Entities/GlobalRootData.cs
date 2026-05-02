using System.Collections.Generic;
using System.Runtime.Serialization;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities;

[DataContract]
public class GlobalRootData
{
    [DataMember(Order = 1)]
    public List<GlobalRootEntity> Entities = new();

    public static GlobalRootData From(List<GlobalRootEntity> entities)
    {
        return new GlobalRootData { Entities = entities };
    }
}

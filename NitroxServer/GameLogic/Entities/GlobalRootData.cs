using System.Collections.Generic;
using System.Runtime.Serialization;
using NitroxModel.DataStructures.GameLogic.Entities;

namespace NitroxServer.GameLogic.Entities;

public class GlobalRootData : EntityData
{
    [DataMember(Order = 1)]
    public new List<GlobalRootEntity> Entities = new();

    public static GlobalRootData From(List<GlobalRootEntity> entities)
    {
        return new GlobalRootData { Entities = entities };
    }
}

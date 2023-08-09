using System.Collections.Generic;
using System.Runtime.Serialization;
using NitroxModel.DataStructures.GameLogic.Entities;

namespace NitroxServer.GameLogic.Entities;

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

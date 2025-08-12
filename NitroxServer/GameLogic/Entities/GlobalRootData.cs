using System.Collections.Generic;
using System.Runtime.Serialization;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using ProtoBufNet;

namespace NitroxServer.GameLogic.Entities;

[DataContract]
public class GlobalRootData
{
    [DataMember(Order = 1)]
    public List<GlobalRootEntity> Entities = new();

    [ProtoAfterDeserialization]
    private void ProtoAfterDeserialization()
    {
        foreach (GlobalRootEntity entity in Entities)
        {
            EnsureChildrenTransformAreParented(entity);
        }
    }

    [OnDeserialized]
    private void JsonAfterDeserialization(StreamingContext context)
    {
        ProtoAfterDeserialization();
    }

    private static void EnsureChildrenTransformAreParented(WorldEntity entity)
    {
        if (entity.Transform == null)
        {
            return;
        }
        foreach (Entity child in entity.ChildEntities)
        {
            if (child is WorldEntity childWE && childWE.Transform != null)
            {
                childWE.Transform.SetParent(entity.Transform, false);
                EnsureChildrenTransformAreParented(childWE);
            }
        }
    }

    public static GlobalRootData From(List<GlobalRootEntity> entities)
    {
        return new GlobalRootData { Entities = entities };
    }
}

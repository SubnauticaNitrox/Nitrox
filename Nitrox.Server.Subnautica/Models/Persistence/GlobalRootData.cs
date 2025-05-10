using System.Collections.Generic;
using System.Runtime.Serialization;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using ProtoBufNet;

namespace Nitrox.Server.Subnautica.Models.Persistence;

[DataContract]
internal class GlobalRootData
{
    [DataMember(Order = 1)]
    public List<GlobalRootEntity> Entities { get; set; } = [];

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
            if (child is WorldEntity { Transform: not null } childWe)
            {
                childWe.Transform.SetParent(entity.Transform, false);
                EnsureChildrenTransformAreParented(childWe);
            }
        }
    }
}

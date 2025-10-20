using System.Collections.Generic;
using System.Runtime.Serialization;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using ProtoBufNet;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities;

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

        bool brokenEntitiesFound = false;

        foreach (Entity child in entity.ChildEntities)
        {
            if (child is not WorldEntity childWE)
            {
                continue;
            }

            if (child is not GlobalRootEntity && childWE.Level == 100)
            {
                Log.Error($"Found a world entity with cell level 100. It will be hotfixed.\n{childWE}");
                childWE.Level = 0;
                brokenEntitiesFound = true;
            }

            if (childWE.Transform != null)
            {
                childWE.Transform.SetParent(entity.Transform, false);
                EnsureChildrenTransformAreParented(childWE);
            }
        }

        if (brokenEntitiesFound)
        {
            Log.WarnOnce($"Please consider reporting the above issues on the Discord server.");
        }
    }

    public static GlobalRootData From(List<GlobalRootEntity> entities)
    {
        return new GlobalRootData { Entities = entities };
    }
}

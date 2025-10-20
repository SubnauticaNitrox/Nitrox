using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using ProtoBufNet;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities;

[DataContract]
public class EntityData
{
    [DataMember(Order = 1)]
    public List<Entity> Entities = [];

    [ProtoAfterDeserialization]
    private void ProtoAfterDeserialization()
    {
        // After deserialization, we want to assign all of the
        // children to their respective parent entities.
        Dictionary<NitroxId, Entity> entitiesById = Entities.ToDictionary(entity => entity.Id);

        bool brokenEntitiesFound = false;

        foreach (Entity entity in Entities)
        {
            if (entity is WorldEntity we)
            {
                NitroxVector3 pos = we.Transform.LocalPosition;
                if (float.IsNaN(pos.X) || float.IsNaN(pos.Y) || float.IsNaN(pos.Z) ||
                    float.IsInfinity(pos.X) || float.IsInfinity(pos.Y) || float.IsInfinity(pos.Z))
                {
                    Log.Error("Found WorldEntity with NaN or infinite position. Teleporting it to world origin.");
                    we.Transform.LocalPosition = NitroxVector3.Zero;
                }

                NitroxQuaternion rot = we.Transform.LocalRotation;
                if (float.IsNaN(rot.X) || float.IsNaN(rot.Y) || float.IsNaN(rot.Z) || float.IsNaN(rot.W) ||
                    float.IsInfinity(rot.X) || float.IsInfinity(rot.Y) || float.IsInfinity(rot.Z) || float.IsInfinity(rot.W))
                {
                    Log.Error("Found WorldEntity with NaN or infinite rotation. Resetting rotation.");
                    we.Transform.LocalRotation = NitroxQuaternion.Identity;
                }

                if (we.Level == 100)
                {
                    Log.Error($"Found a world entity with cell level 100. It will be hotfixed.\n{we}");
                    we.Level = 0;
                    brokenEntitiesFound = true;
                }
            }

            // We will re-build the child hierarchy below and want to avoid duplicates.
            // TODO: Rework system to no longer persist children entities because they are duplicates.
            entity.ChildEntities.Clear();
        }

        if (brokenEntitiesFound)
        {
            Log.WarnOnce($"Please consider reporting the above issues on the Discord server.");
        }

        foreach (Entity entity in Entities)
        {
            if (entity.ParentId != null && entitiesById.TryGetValue(entity.ParentId, out Entity parentEntity))
            {
                parentEntity.ChildEntities.Add(entity);

                if (entity is WorldEntity worldEntity && parentEntity is WorldEntity parentWorldEntity)
                {
                    worldEntity.Transform.SetParent(parentWorldEntity.Transform, false);
                }
            }
        }
    }

    [OnDeserialized]
    private void JsonAfterDeserialization(StreamingContext context)
    {
        ProtoAfterDeserialization();
    }

    public static EntityData From(List<Entity> entities)
    {
        return new EntityData { Entities = entities };
    }
}

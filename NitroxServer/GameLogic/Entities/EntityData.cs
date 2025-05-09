using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Unity;
using ProtoBufNet;

namespace NitroxServer.GameLogic.Entities;

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
            }

            // We will re-build the child hierarchy below and want to avoid duplicates.
            // TODO: Rework system to no longer persist children entities because they are duplicates.
            entity.ChildEntities.Clear();

            if (entity.ParentId == null)
            {
                continue;
            }

            if (entitiesById.TryGetValue(entity.ParentId, out Entity parent))
            {
                parent.ChildEntities.Add(entity);

                if (entity is WorldEntity we2 && parent is WorldEntity weParent)
                {
                    we2.Transform.SetParent(weParent.Transform, false);
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

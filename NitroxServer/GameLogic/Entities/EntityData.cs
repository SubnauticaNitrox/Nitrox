using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;

namespace NitroxServer.GameLogic.Entities
{
    [ProtoContract]
    public class EntityData
    {
        [ProtoMember(1)]
        public List<Entity> Entities = new List<Entity>();

        [ProtoAfterDeserialization]
        public void ProtoAfterDeserialization()
        {
            // After deserialziation, we want to assign all of the 
            // children to their respective parent entities.
            Dictionary<NitroxId, Entity> entitiesById = Entities.ToDictionary(entity => entity.Id);

            foreach(Entity entity in Entities)
            {
                if(entity.ParentId != null)
                {
                    Entity parent = entitiesById[entity.ParentId];

                    if(parent != null)
                    {
                        parent.ChildEntities.Add(entity);
                        entity.Transform.SetParent(parent.Transform);
                    }
                }
            }
        }

        public static EntityData From(List<Entity> entities)
        {
            EntityData entityData = new EntityData();
            entityData.Entities = entities;

            return entityData;
        }
    }
}

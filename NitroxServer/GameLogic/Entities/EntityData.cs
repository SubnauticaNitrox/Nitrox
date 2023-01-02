using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using ProtoBufNet;

namespace NitroxServer.GameLogic.Entities
{
    [DataContract]
    public class EntityData
    {
        [DataMember(Order = 1)]
        public List<Entity> Entities = new List<Entity>();

        [ProtoAfterDeserialization]
        private void ProtoAfterDeserialization()
        {
            // After deserialization, we want to assign all of the 
            // children to their respective parent entities.
            Dictionary<NitroxId, Entity> entitiesById = Entities.ToDictionary(entity => entity.Id);

            foreach (Entity entity in Entities)
            {
                if (entity.ParentId != null)
                {
                    if (entitiesById.TryGetValue(entity.ParentId, out Entity parent))
                    {
                        parent.ChildEntities.Add(entity);

                        if (entity is WorldEntity we && parent is WorldEntity weParent)
                        {
                            we.Transform.SetParent(weParent.Transform);
                        }
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
}

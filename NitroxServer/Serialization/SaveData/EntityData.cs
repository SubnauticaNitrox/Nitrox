using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.Serialization.SaveData
{
    [JsonObject(MemberSerialization.OptIn)]
    public class EntityData
    {
        [JsonProperty]
        public List<Entity> Entities = new();

        [OnDeserialized]
        private void JsonAfterDeserialization(StreamingContext context)
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
                        entity.Transform.SetParent(parent.Transform, false);
                    }
                }
            }
        }


        public static EntityData From(List<Entity> entities)
        {
            return new EntityData { Entities = entities };
        }
    }
}

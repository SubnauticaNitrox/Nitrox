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
        public List<NitroxObject> Entities = new List<NitroxObject>();

        public static EntityData From(List<Entity> entities)
        {
            EntityData entityData = new EntityData();
            entityData.Entities = entities.Select(e => e.NitroxObject).ToList();

            return entityData;
        }
    }
}

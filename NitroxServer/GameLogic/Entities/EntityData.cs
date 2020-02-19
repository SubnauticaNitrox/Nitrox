using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;

namespace NitroxServer.GameLogic.Entities
{
    [ProtoContract]
    public class EntityData
    {
        [ProtoMember(1)]
        public List<Entity> Entities = new List<Entity>();

        public static EntityData From(List<Entity> entities)
        {
            EntityData entityData = new EntityData();
            entityData.Entities = entities;

            return entityData;
        }
    }
}

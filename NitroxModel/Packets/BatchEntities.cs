using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class BatchEntities : Packet
    {
        public List<Entity> Entities { get; }

        public bool ForceRespawn { get; }

        public BatchEntities(List<Entity> entities)
        {
            Entities = entities;
            ForceRespawn = false;
        }

        public BatchEntities(Entity entity)
        {
            Entities = new List<Entity>
            {
                entity
            };

            ForceRespawn = false;
        }

        public BatchEntities(Entity entity, bool forceRespawn)
        {
            Entities = new List<Entity>
            {
                entity
            };

            ForceRespawn = forceRespawn;
        }

        // Constructor for serialization. 
        public BatchEntities(List<Entity> entities, bool forceRespawn)
        {
            Entities = entities;
            ForceRespawn = forceRespawn;
        }
    }
}

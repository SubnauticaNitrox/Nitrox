using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SpawnEntities : Packet
    {
        public List<Entity> Entities { get; }

        public bool ForceRespawn { get; }

        public SpawnEntities(List<Entity> entities)
        {
            Entities = entities;
            ForceRespawn = false;
        }

        public SpawnEntities(Entity entity)
        {
            Entities = new List<Entity>
            {
                entity
            };

            ForceRespawn = false;
        }

        public SpawnEntities(Entity entity, bool forceRespawn)
        {
            Entities = new List<Entity>
            {
                entity
            };

            ForceRespawn = forceRespawn;
        }

        // Constructor for serialization. 
        public SpawnEntities(List<Entity> entities, bool forceRespawn)
        {
            Entities = entities;
            ForceRespawn = forceRespawn;
        }
    }
}

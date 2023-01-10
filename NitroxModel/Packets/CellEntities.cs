using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CellEntities : Packet
    {
        public List<Entity> Entities { get; }

        public bool ForceRespawn { get; }

        public CellEntities(List<Entity> entities)
        {
            Entities = entities;
            ForceRespawn = false;
        }

        public CellEntities(Entity entity)
        {
            Entities = new List<Entity>
            {
                entity
            };

            ForceRespawn = false;
        }

        public CellEntities(Entity entity, bool forceRespawn)
        {
            Entities = new List<Entity>
            {
                entity
            };

            ForceRespawn = forceRespawn;
        }

        // Constructor for serialization. 
        public CellEntities(List<Entity> entities, bool forceRespawn)
        {
            Entities = entities;
            ForceRespawn = forceRespawn;
        }
    }
}

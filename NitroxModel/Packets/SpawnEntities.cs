using NitroxModel.GameLogic;
using System;
using System.Collections.Generic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SpawnEntities : Packet
    {
        public List<SpawnedEntity> Entities { get; }

        public SpawnEntities(List<SpawnedEntity> entities) : base()
        {
            Entities = entities;
        }

        public override string ToString()
        {
            String toString = "[SpawnEntities ";

            foreach(SpawnedEntity entity in Entities)
            {
                toString += entity;
            }

            return toString + "]";
        }
    }
}

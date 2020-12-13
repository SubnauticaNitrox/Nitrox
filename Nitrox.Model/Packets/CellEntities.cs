using System;
using System.Collections.Generic;
using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class CellEntities : Packet
    {
        public List<Entity> Entities { get; }

        public CellEntities(List<Entity> entities)
        {
            Entities = entities;
        }

        public CellEntities(Entity entity)
        {
            Entities = new List<Entity>
            {
                entity
            };
        }

        public override string ToString()
        {
            string toString = "[CellEntities ";

            foreach (Entity entity in Entities)
            {
                toString += entity;
            }

            return toString + "]";
        }
    }
}

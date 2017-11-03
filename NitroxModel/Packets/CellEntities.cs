using NitroxModel.GameLogic;
using System;
using System.Collections.Generic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CellEntities : Packet
    {
        public List<Entity> Entities { get; }

        public CellEntities(List<Entity> entities) : base()
        {
            Entities = entities;
        }

        public override string ToString()
        {
            String toString = "[CellEntities ";

            foreach(Entity entity in Entities)
            {
                toString += entity;
            }

            return toString + "]";
        }
    }
}

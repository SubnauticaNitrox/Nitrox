using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class CellEntities : Packet
    {
        [Index(0)]
        public virtual List<Entity> Entities { get; protected set; }

        private CellEntities() { }

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
    }
}

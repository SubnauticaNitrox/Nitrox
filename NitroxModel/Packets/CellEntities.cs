using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CellEntities : Packet
    {
        public List<WorldEntity> Entities { get; }

        public CellEntities(List<WorldEntity> entities)
        {
            Entities = entities;
        }

        public CellEntities(WorldEntity entity)
        {
            Entities = new List<WorldEntity>
            {
                entity
            };
        }
    }
}

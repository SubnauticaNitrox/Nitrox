using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;
using UWE;

namespace NitroxClient.Communication.Packets.Processors
{
    class CellEntitiesProcessor : ClientPacketProcessor<CellEntities>
    {
        private readonly Entities entities;

        public CellEntitiesProcessor(Entities entities)
        {
            this.entities = entities;
        }

        public override void Process(CellEntities packet)
        {
            List<WorldEntity> worldEntities = packet.Entities.Cast<WorldEntity>().ToList();
            CoroutineHost.StartCoroutine(entities.SpawnAsync(worldEntities));
        }
    }
}

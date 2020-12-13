using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
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
            entities.Spawn(packet.Entities);
        }
    }
}

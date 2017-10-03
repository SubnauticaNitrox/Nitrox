using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    class CreatureActionChangedProcessor : ClientPacketProcessor<CreatureActionChanged>
    {
        public override void Process(CreatureActionChanged packet)
        {
            // throw new System.NotImplementedException();
        }
    }
}

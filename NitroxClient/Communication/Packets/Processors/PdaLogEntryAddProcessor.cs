using NitroxClient.Communication.Abstract;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PdaLogEntryAddProcessor : IClientPacketProcessor<PdaLogEntryAdd>
    {
        private readonly IPacketSender packetSender;

        public PdaLogEntryAddProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public Task Process(IPacketProcessContext context, PdaLogEntryAdd packet)
        {
            using (PacketSuppressor<PdaLogEntryAdd>.Suppress())
            {
                PDALog.Add(packet.Key);
            }

            return Task.CompletedTask;
        }
    }
}

using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PDALogEntryAddProcessor : ClientPacketProcessor<PDALogEntryAdd>
    {
        private readonly IPacketSender packetSender;

        public PDALogEntryAddProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(PDALogEntryAdd packet)
        {
            using (packetSender.Suppress<PDALogEntryAddProcessor>())
            {
                Dictionary<string, PDALog.Entry> entries = PDALog.entries;

                if (!entries.ContainsKey(packet.Key))
                {

                    if (!PDALog.GetEntryData(packet.Key, out PDALog.EntryData entryData))
                    {
                        entryData = new PDALog.EntryData();
                        entryData.key = packet.Key;
                        entryData.type = PDALog.EntryType.Invalid;
                    }

                    PDALog.Entry entry = new PDALog.Entry();
                    entry.data = entryData;
                    entry.timestamp = packet.Timestamp;
                    entries.Add(entryData.key, entry);
                }
            }
        }
    }
}

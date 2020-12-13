using System.Collections.Generic;
using System.Reflection;
using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
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
                Dictionary<string, PDALog.Entry> entries = (Dictionary<string, PDALog.Entry>)(typeof(PDALog).GetField("entries", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
                
                if (!entries.ContainsKey(packet.Key))
                {
                    PDALog.EntryData entryData;

                    if (!PDALog.GetEntryData(packet.Key, out entryData))
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

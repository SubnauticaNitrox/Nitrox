using System.Collections.Generic;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PDAScannerEntryRemoveProcessor : ClientPacketProcessor<PDAEntryRemove>
    {
        private readonly IPacketSender packetSender;

        public PDAScannerEntryRemoveProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(PDAEntryRemove packet)
        {
            using (packetSender.Suppress<PDAEntryRemove>())
            {

                if (PDAScanner.GetPartialEntryByKey(packet.TechType.ToUnity(), out PDAScanner.Entry entry))
                {
                    List<PDAScanner.Entry> partial = (List<PDAScanner.Entry>)(typeof(PDAScanner).GetField("partial", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
                    HashSet<TechType> complete = (HashSet<TechType>)(typeof(PDAScanner).GetField("complete", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
                    partial.Remove(entry);
                    complete.Add(entry.techType);
                }
            }
        }
    }
}

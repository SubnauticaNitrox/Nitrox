using System.Collections.Generic;
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
                    List<PDAScanner.Entry> partial = PDAScanner.partial;
                    HashSet<TechType> complete = PDAScanner.complete;
                    partial.Remove(entry);
                    complete.Add(entry.techType);
                }
            }
        }
    }
}

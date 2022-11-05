using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PDAScannerEntryRemoveProcessor : ClientPacketProcessor<PDAEntryRemove>
    {
        public override void Process(PDAEntryRemove packet)
        {
            if (PDAScanner.GetPartialEntryByKey(packet.TechType.ToUnity(), out PDAScanner.Entry entry))
            {
                PDAScanner.partial.Remove(entry);
                PDAScanner.complete.Add(entry.techType);
            }
        }
    }
}

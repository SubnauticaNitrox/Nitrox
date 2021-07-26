using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel_Subnautica.Helper;

namespace NitroxClient.GameLogic
{
    public class PDAManagerEntry
    {
        private readonly IPacketSender packetSender;

        public PDAManagerEntry(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void Add(PDAScanner.Entry entry)
        {
            PDAEntryAdd EntryChanged = new PDAEntryAdd(entry.techType.ToDto(), entry.progress,entry.unlocked);
            packetSender.Send(EntryChanged);
        }

        public void Progress(PDAScanner.Entry entry)
        {
            PDAEntryProgress EntryChanged = new PDAEntryProgress(entry.techType.ToDto(), entry.progress, entry.unlocked);
            packetSender.Send(EntryChanged);
        }

        public void Remove(PDAScanner.Entry entry)
        {
            PDAEntryRemove EntryChanged = new PDAEntryRemove(entry.techType.ToDto(), entry.progress, entry.unlocked);
            packetSender.Send(EntryChanged);
        }

        public void LogAdd(PDALog.Entry entry)
        {
            PDALogEntryAdd EntryChanged = new PDALogEntryAdd(entry.data.key, entry.timestamp);
            packetSender.Send(EntryChanged);
        }
    }
}

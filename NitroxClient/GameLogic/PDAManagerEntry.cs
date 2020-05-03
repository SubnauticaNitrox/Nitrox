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
            PDAEntryAdd entryChanged = new PDAEntryAdd(entry.techType.ToDto(), entry.progress,entry.unlocked);
            packetSender.Send(entryChanged);
        }

        public void Progress(PDAScanner.Entry entry)
        {
            PDAEntryProgress entryChanged = new PDAEntryProgress(entry.techType.ToDto(), entry.progress, entry.unlocked);
            packetSender.Send(entryChanged);
        }

        public void Remove(PDAScanner.Entry entry)
        {
            PDAEntryRemove entryChanged = new PDAEntryRemove(entry.techType.ToDto(), entry.progress, entry.unlocked);
            packetSender.Send(entryChanged);
        }

        public void LogAdd(PDALog.Entry entry)
        {
            PDALogEntryAdd entryChanged = new PDALogEntryAdd(entry.data.key, entry.timestamp);
            packetSender.Send(entryChanged);
        }
    }
}
